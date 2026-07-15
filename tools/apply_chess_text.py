# -*- coding: utf-8
"""Apply chess text from chess_text_data to PropertyCreator .asset files (fill empty fields only)."""
import json
import os
import re
import sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(__file__), ".."))
PLAYER_DIR = os.path.join(ROOT, "Assets", "Resources", "ChessData", "Player")
ENEMY_DIR = os.path.join(ROOT, "Assets", "Resources", "ChessData", "Enemy")

sys.path.insert(0, os.path.dirname(__file__))
from chess_text_data import PLANTS, ENEMIES  # noqa: E402
from audit_content_config import field_nonempty, parse_chess_name  # noqa: E402

TEXT_FIELDS = ("chessDescription", "chessShortDescription", "chessEffect")
NEXT_KEY_PREFIXES = (
    "chess",
    "baseProperty",
    "plantTags",
    "fetterMemberId",
    "chessTileType",
    "chessPre",
    "chessSprite",
)


def yaml_quote(s):
    s = str(s).replace("\\", "\\\\").replace('"', '\\"')
    return f'"{s}"'


def is_empty_yaml_value(rest):
    rest = rest.strip()
    if not rest:
        return True
    if rest.endswith(":"):
        return True
    return any(rest.startswith(p) for p in NEXT_KEY_PREFIXES)


def has_quoted_value(rest):
    rest = rest.strip()
    return rest.startswith('"') and rest.endswith('"') and len(rest) > 2


def ensure_text_fields_lines(lines):
    """Return lines with chessDescription/Short/Effect inserted after chessName if missing."""
    names_present = set()
    chess_name_idx = None
    for i, line in enumerate(lines):
        m = re.match(r"^\s*(chessDescription|chessShortDescription|chessEffect):\s*", line)
        if m:
            names_present.add(m.group(1))
        if chess_name_idx is None and re.match(r"^\s*chessName:", line):
            chess_name_idx = i
    missing = [f for f in TEXT_FIELDS if f not in names_present]
    if not missing or chess_name_idx is None:
        return lines
    insert = [f"  {f}: \n" for f in missing]
    return lines[: chess_name_idx + 1] + insert + lines[chess_name_idx + 1 :]


def set_field_on_lines(lines, field, value):
    quoted = yaml_quote(value)
    changed = False
    out = []
    for line in lines:
        m = re.match(rf"^(\s*{re.escape(field)}:\s*)(.*)$", line.rstrip("\r\n"))
        if m and not has_quoted_value(m.group(2)) and is_empty_yaml_value(m.group(2)):
            out.append(f"{m.group(1)}{quoted}\n")
            changed = True
        else:
            out.append(line if line.endswith("\n") else line + "\n")
    return out, changed


def apply_to_content(content, entry):
    lines = content.splitlines(keepends=True)
    if not lines:
        return content, 0
    lines = ensure_text_fields_lines(lines)
    fields_set = 0
    for field in TEXT_FIELDS:
        val = entry.get(field)
        if not val:
            continue
        if field_nonempty("".join(lines), field):
            continue
        lines, changed = set_field_on_lines(lines, field, val)
        if changed:
            fields_set += 1
    return "".join(lines), fields_set


def apply_catalog_to_dir(dir_path, catalog):
    stats = {"files": 0, "updated": 0, "skipped_no_catalog": [], "fields_set": 0}
    for fn in sorted(os.listdir(dir_path)):
        if not fn.endswith(".asset"):
            continue
        path = os.path.join(dir_path, fn)
        with open(path, encoding="utf-8") as f:
            content = f.read()
        name = parse_chess_name(content, fn)
        entry = catalog.get(name)
        if not entry:
            stats["skipped_no_catalog"].append(name)
            continue
        stats["files"] += 1
        new_content, n = apply_to_content(content, entry)
        if n:
            with open(path, "w", encoding="utf-8", newline="\n") as f:
                f.write(new_content)
            stats["updated"] += 1
            stats["fields_set"] += n
    return stats


def export_catalog_json():
    out = os.path.join(os.path.dirname(__file__), "chess_text_catalog.json")
    with open(out, "w", encoding="utf-8") as f:
        json.dump({"plants": PLANTS, "enemies": ENEMIES}, f, ensure_ascii=False, indent=2)
    return out


def main():
    export_catalog_json()
    ps = apply_catalog_to_dir(PLAYER_DIR, PLANTS)
    es = apply_catalog_to_dir(ENEMY_DIR, ENEMIES)
    print("=== apply_chess_text ===")
    print(f"Plants: {ps['updated']}/{ps['files']} files updated, {ps['fields_set']} fields set")
    if ps["skipped_no_catalog"]:
        print("  Plants missing catalog:", ", ".join(ps["skipped_no_catalog"]))
    print(f"Enemies: {es['updated']}/{es['files']} files updated, {es['fields_set']} fields set")
    if es["skipped_no_catalog"]:
        print("  Enemies missing catalog:", ", ".join(es["skipped_no_catalog"]))


if __name__ == "__main__":
    main()
