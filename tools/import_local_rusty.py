"""Import model/texture/prefab data from the user's cached Unity package, without scripts."""
import json
import hashlib
from pathlib import Path, PurePosixPath
import tarfile

root = Path(__file__).resolve().parents[1]
cache = Path.home() / 'AppData/Roaming/Unity/Asset Store-5.x'
package = next(cache.rglob('Sci-Fi Old Rusty Props PBR.unitypackage'))
prefix = 'Assets/Data/Sci-fi_assets/Sci-Fi Old rusty props PBR'
allowed = {'.fbx', '.png', '.jpg', '.tga', '.mat', '.prefab'}
written = []
with tarfile.open(package) as archive:
    members = {m.name: m for m in archive.getmembers()}
    for name, member in members.items():
        if not name.endswith('/pathname'): continue
        relative = archive.extractfile(member).read().decode('utf-8').splitlines()[0].rstrip('\0')
        path = PurePosixPath(relative)
        if '..' in path.parts or path.is_absolute(): raise ValueError('Unsafe package path')
        if not (relative == prefix or relative.startswith(prefix + '/') or relative in ('Assets/Data', 'Assets/Data/Sci-fi_assets')): continue
        target = (root / relative).resolve()
        if not target.is_relative_to(root / 'Assets'): raise ValueError('Outside Assets')
        guid = name.rsplit('/', 1)[0]
        data = members.get(guid + '/asset')
        if data is not None and path.suffix.lower() not in allowed: continue
        if data is None: target.mkdir(parents=True, exist_ok=True)
        else:
            target.parent.mkdir(parents=True, exist_ok=True)
            content = archive.extractfile(data).read()
            if target.exists() and target.read_bytes() != content: raise ValueError('Preserve edited local asset: ' + relative)
            target.write_bytes(content)
            written.append({'path': relative, 'sha256': hashlib.sha256(content).hexdigest()})
        meta = members.get(guid + '/asset.meta')
        if meta is not None:
            destination = Path(str(target) + '.meta')
            content = archive.extractfile(meta).read()
            if destination.exists() and destination.read_bytes() != content: raise ValueError('Preserve existing GUID: ' + relative)
            destination.write_bytes(content)
report = {'pack': 'Sci-Fi Old Rusty Props PBR', 'source': 'Local Unity Asset Store cache supplied by user',
    'url': 'https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-old-rusty-props-pbr-200267',
    'license': 'Unity Asset Store EULA; source assets remain local', 'packageSha256': hashlib.sha256(package.read_bytes()).hexdigest(), 'files': written}
(root / 'docs/local-rusty-manifest.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print('Imported', len(written), 'model/texture/material/prefab files; no scripts or demo scenes.')
