from pathlib import Path
import json
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
PACKAGES = {
    'com.ni.tengine': ('com.ni.tengine', '0.6.0'),
    'com.ni.gamecore': ('com.ni.gamecore', '0.1.0'),
    'com.cysharp.unitask': ('ThirdParty/UniTask', '2.5.11'),
    'com.tuyoogame.yooasset': ('ThirdParty/YooAsset', '2.3.17'),
    'com.code-philosophy.hybridclr': ('ThirdParty/HybridCLR', '8.8.0'),
}

def main():
    errors = []
    guids = {}
    files = 0
    for name, (relative, version) in PACKAGES.items():
        folder = ROOT / relative
        metadata = json.loads((folder/'package.json').read_text(encoding='utf-8-sig'))
        if metadata.get('name') != name or metadata.get('version') != version:
            errors.append('Unexpected identity/version: ' + relative)
        if not list(folder.glob('*LICENSE*')):
            errors.append('Missing package license: ' + relative)
        for dependency, expected in metadata.get('dependencies', {}).items():
            if expected.startswith(('file:', 'http:', 'https:', 'git:', 'ssh:')):
                errors.append('Non-registry package dependency: ' + name + ' -> ' + dependency)
            if dependency in PACKAGES and expected != PACKAGES[dependency][1]:
                errors.append('Dependency version mismatch: ' + name + ' -> ' + dependency)
            if dependency not in PACKAGES and not dependency.startswith('com.unity.'):
                errors.append('Dependency missing from installation set: ' + dependency)
        for path in folder.rglob('*'):
            if not path.is_file() or any(part.endswith('~') or part.startswith('.') for part in path.relative_to(folder).parts):
                continue
            files += 1
            if path.suffix == '.cs' and not Path(str(path)+'.meta').exists():
                errors.append('Missing script meta: ' + str(path.relative_to(ROOT)))
            if path.suffix == '.meta':
                match = re.search(r'^guid: ([0-9a-fA-F]{32})\s*$', path.read_text(encoding='utf-8-sig'), re.M)
                if not match:
                    errors.append('Invalid meta GUID: ' + str(path.relative_to(ROOT)))
                elif match[1] in guids:
                    errors.append('Duplicate GUID: ' + str(path.relative_to(ROOT)) + ' / ' + guids[match[1]])
                else:
                    guids[match[1]] = str(path.relative_to(ROOT))
            if path.stat().st_size >= 100 * 1024 * 1024:
                errors.append('Oversized Git input: ' + str(path.relative_to(ROOT)))
    print(json.dumps({'success':not errors,'packages':len(PACKAGES),'files':files,'guids':len(guids),'errors':errors},indent=2))
    return 0 if not errors else 1

if __name__ == '__main__':
    sys.exit(main())
