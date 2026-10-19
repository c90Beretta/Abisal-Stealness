"""Fast, offline checks for references, license records, GUIDs, and tracked caches."""
import hashlib
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def main():
    errors = []
    manifest = ROOT / "docs/asset-install-manifest.json"
    if not manifest.exists():
        errors.append("Missing asset install manifest")
    else:
        for pack in json.loads(manifest.read_text(encoding="utf-8")):
            folder = ROOT / "Assets/ThirdParty/Kenney" / pack["pack"]
            for name in pack["files"]:
                if not (folder / name).is_file():
                    errors.append(f"Missing installed asset: {folder / name}")
            for name, expected in pack.get("fileSha256", {}).items():
                path = folder / name
                if path.is_file() and hashlib.sha256(path.read_bytes()).hexdigest() != expected:
                    errors.append(f"Changed third party file: {path}")
    font_manifest = ROOT / "docs/font-install-manifest.json"
    if font_manifest.exists():
        for item in json.loads(font_manifest.read_text(encoding="utf-8")):
            path = ROOT / item["file"]
            if not path.is_file() or hashlib.sha256(path.read_bytes()).hexdigest() != item["sha256"]:
                errors.append(f"Missing or changed font/license: {item['file']}")
    guids = {}
    assets = ROOT / "Assets"
    for path in assets.rglob("*"):
        if path.suffix != ".meta" and not Path(str(path) + ".meta").is_file():
            errors.append(f"Missing .meta: {path.relative_to(ROOT)}")
        if path.suffix == ".meta":
            match = re.search(r"^guid: ([a-f0-9]{32})$", path.read_text(encoding="utf-8"), re.M)
            if match:
                guid = match[1]
                if guid in guids:
                    errors.append(f"Duplicate GUID: {path} / {guids[guid]}")
                guids[guid] = path
            else:
                errors.append(f"Missing or invalid GUID: {path.relative_to(ROOT)}")
            if not Path(str(path)[:-5]).exists():
                errors.append(f"Orphan .meta: {path.relative_to(ROOT)}")
    lock = json.loads((ROOT / "Packages/packages-lock.json").read_text())['dependencies']
    deps = json.loads((ROOT / "Packages/manifest.json").read_text())['dependencies']
    for name, version in deps.items():
        if lock.get(name, {}).get('version') != version:
            errors.append(f"Unresolved package: {name}@{version}")
    tracked = subprocess.check_output(["git", "ls-files"], cwd=ROOT, text=True).splitlines()
    for name in tracked:
        if name.split('/')[0].lower() in {"library", "temp", "logs", ".plastic", "builds", "usersettings"}:
            errors.append(f"Tracked generated directory: {name}")
    if errors:
        print('\n'.join(errors))
        raise SystemExit(1)
    print(f"PASS: {len(guids)} unique GUIDs; installed files and package locks verified; no tracked caches.")


if __name__ == '__main__':
    main()
