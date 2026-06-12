import glob
import os
import re

BASE = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "LevelData", "RogueMode")
SCENES = ["前院", "Ring", "学校"]


def parse_asset(path):
    with open(path, encoding="utf-8") as f:
        text = f.read()
    m_name = re.search(r'm_Name: "(.+)"', text)
    m_diff = re.search(r"roguelikeDifficulty: ([\d.]+)", text)
    m_kind = re.search(r"roguelikeKind: (\d+)", text)
    if not m_diff:
        return None
    name = m_name.group(1) if m_name else os.path.basename(path)
    try:
        name = name.encode("utf-8").decode("unicode_escape")
    except Exception:
        pass
    return {
        "path": path,
        "name": name,
        "difficulty": float(m_diff.group(1)),
        "kind": int(m_kind.group(1)) if m_kind else 0,
    }


def set_nextlevel(path, next_guid):
    with open(path, encoding="utf-8") as f:
        text = f.read()
    if next_guid:
        new_line = f"  nextLevel: {{fileID: 11400000, guid: {next_guid}, type: 2}}"
    else:
        new_line = "  nextLevel: {fileID: 0}"
    new_text, n = re.subn(
        r"  nextLevel: \{fileID: [^}]+\}",
        new_line,
        text,
        count=1,
    )
    if n != 1:
        print(f"WARN nextLevel replace count={n} in {path}")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(new_text)


def guid_for(asset_path):
    meta = asset_path + ".meta"
    with open(meta, encoding="utf-8") as f:
        m = re.search(r"guid: (\w+)", f.read())
    return m.group(1) if m else None


def main():
    for scene in SCENES:
        folder = os.path.join(BASE, scene)
        assets = []
        for path in glob.glob(os.path.join(folder, "*.asset")):
            info = parse_asset(path)
            if info is None:
                continue
            info["guid"] = guid_for(path)
            assets.append(info)

        chain = [a for a in assets if a["kind"] != 3]
        chain.sort(key=lambda a: (a["difficulty"], a["kind"], a["name"]))

        print(f"\n=== {scene} ({len(chain)} levels; Boss excluded) ===")
        for i, a in enumerate(chain):
            nxt = chain[i + 1]["guid"] if i + 1 < len(chain) else None
            nxt_name = chain[i + 1]["name"] if nxt else "(end)"
            print(f"  {a['difficulty']:>4} kind={a['kind']} {a['name']} -> {nxt_name}")
            set_nextlevel(a["path"], nxt)

        for b in [a for a in assets if a["kind"] == 3]:
            set_nextlevel(b["path"], None)
            print(f"  [Boss terminal] {b['name']}")

    print("\nDone.")


if __name__ == "__main__":
    main()
