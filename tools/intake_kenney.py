"""Download a small, licensed MVP subset from Kenney's official distribution.

First run with --inspect to inspect archive names; installation requires the
checked-in allowlist. Original archives stay in ignored Library/.
"""
import argparse
import concurrent.futures
import hashlib
import io
import json
import re
import urllib.request
import zipfile
from html import unescape
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PACKS = {
    "modular-space-kit": "ModularSpaceKit",
    "blaster-kit": "BlasterKit",
    "impact-sounds": "ImpactSounds",
    "animated-characters-survivors": "Survivors",
}


def fetch(url):
    req = urllib.request.Request(url, headers={"User-Agent": "Zombtoy-asset-intake/1.0"})
    with urllib.request.urlopen(req, timeout=60) as response:
        return response.read()


def download_archive(url):
    # The official host throttles long connections. Six bounded range requests
    # keep downloads usable without alternate mirrors or aggressive concurrency.
    with urllib.request.urlopen(urllib.request.Request(url, method="HEAD"), timeout=30) as r:
        size = int(r.headers["Content-Length"])
    chunk = 128 * 1024
    def part(start):
        end = min(start + chunk, size) - 1
        req = urllib.request.Request(url, headers={"Range": f"bytes={start}-{end}"})
        for attempt in range(3):
            try:
                with urllib.request.urlopen(req, timeout=40) as r:
                    if r.status != 206 or r.headers.get("Content-Range") != f"bytes {start}-{end}/{size}":
                        raise RuntimeError("Server did not honor requested range")
                    data = r.read()
                    if len(data) != end - start + 1:
                        raise RuntimeError("Truncated download")
                    return data
            except (OSError, RuntimeError):
                if attempt == 2:
                    raise
    with concurrent.futures.ThreadPoolExecutor(max_workers=6) as executor:
        return b"".join(executor.map(part, range(0, size, chunk)))


def main():
    args = argparse.ArgumentParser()
    args.add_argument("--inspect", action="store_true")
    options = args.parse_args()
    cache = ROOT / "Library/ZombtoyIntake"
    cache.mkdir(parents=True, exist_ok=True)
    allowlist_file = ROOT / "tools/asset_allowlist.json"
    allowlist = json.loads(allowlist_file.read_text()) if allowlist_file.exists() else {}
    manifest_file = ROOT / "docs/asset-install-manifest.json"
    pins = {p["page"]: p for p in json.loads(manifest_file.read_text(encoding="utf-8"))} if manifest_file.exists() else {}
    installed = []
    for slug, folder in PACKS.items():
        page_url = f"https://kenney.nl/assets/{slug}"
        if page_url in pins:
            url = pins[page_url]["download"]
        else:
            page = fetch(page_url).decode("utf-8")
            urls = re.findall(r'href=[\"\x27]([^\"\x27]+)[\"\x27]', page)
            urls = [unescape(u) for u in urls if ".zip" in u]
            if not urls:
                raise RuntimeError(f"No official ZIP found on {page_url}")
            url = urls[-1]
            if url.startswith("/"):
                url = "https://kenney.nl" + url
        if not url.startswith(f"https://kenney.nl/media/pages/assets/{slug}/"):
            raise RuntimeError("Unexpected distribution host; review the source before downloading")
        archive = cache / f"{slug}.zip"
        if not archive.exists() or not zipfile.is_zipfile(archive):
            print(f"Downloading {slug} from official host...", flush=True)
            archive.write_bytes(download_archive(url))
        raw = archive.read_bytes()
        if page_url in pins and hashlib.sha256(raw).hexdigest() != pins[page_url]["archiveSha256"]:
            raise RuntimeError(f"Archive hash mismatch for pinned pack: {slug}")
        with zipfile.ZipFile(io.BytesIO(raw)) as z:
            if options.inspect:
                print(f"\n{slug}: {url}, sha256={hashlib.sha256(raw).hexdigest()}")
                print("\n".join(z.namelist()))
                continue
            selected = allowlist[slug]
            dest = ROOT / "Assets/ThirdParty/Kenney" / folder
            dest.mkdir(parents=True, exist_ok=True)
            licenses = [n for n in z.namelist() if Path(n).name.lower() in ("license.txt", "license.md")]
            if not licenses or not any("CC0" in z.read(n).decode("utf-8-sig") for n in licenses):
                raise RuntimeError(f"Missing CC0 evidence: {slug}")
            for name in [*selected, *licenses]:
                target = (dest / name).resolve()
                if not target.is_relative_to(dest.resolve()):
                    raise RuntimeError("Unsafe archive path")
                target.parent.mkdir(parents=True, exist_ok=True)
                data = z.read(name)
                if target.exists() and target.read_bytes() != data:
                    raise RuntimeError(f"Refusing to overwrite modified asset {target}")
                target.write_bytes(data)
            installed.append({"pack": folder, "page": page_url, "download": url,
                              "archiveSha256": hashlib.sha256(raw).hexdigest(),
                              "license": "CC0-1.0", "author": "Kenney", "files": selected + licenses,
                              "fileSha256": {n: hashlib.sha256(z.read(n)).hexdigest() for n in selected + licenses}})
            print(f"Installed {folder}: {len(selected)} selected files + license")
    if not options.inspect:
        target = ROOT / "docs/asset-install-manifest.json"
        target.parent.mkdir(exist_ok=True)
        target.write_text(json.dumps(installed, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
