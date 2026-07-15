# -*- coding: utf-8 -*-
"""Scan PropertyCreator / Fetter SO for missing text and basic config gaps."""
import json
import os
import re
from datetime import date

ROOT = os.path.normpath(os.path.join(os.path.dirname(__file__), ".."))

PLAYER_DIR = os.path.join(ROOT, "Assets", "Resources", "ChessData", "Player")
ENEMY_DIR = os.path.join(ROOT, "Assets", "Resources", "ChessData", "Enemy")
FETTER_DIR = os.path.join(ROOT, "Assets", "SO", "Fetter")
OUT_JSON = os.path.join(os.path.dirname(__file__), "content_config_gaps.json")
OUT_MD = os.path.join(ROOT, "docs", "game-design", "content-config-audit.md")


def read_text(path):
    with open(path, encoding="utf-8") as f:
        return f.read()


def decode_unicode_name(raw):
    if not raw:
        return raw
    if "\\u" in raw:
        try:
            return raw.encode("utf-8").decode("unicode_escape")
        except UnicodeError:
            pass
    return raw


def field_nonempty(content, field_name):
    """Return True if a Unity YAML scalar field has non-empty text.

    Unity serializes empty strings as ``field:\\n`` (value on the next line absent).
    A naive ``(.*)$`` capture falsely treats the *next* YAML key as the value
    (e.g. ``chessDescription:`` → ``chessShortDescription:``).
    """
    m = re.search(rf"^\s*{re.escape(field_name)}:\s*(.*)$", content, re.M)
    if not m:
        return False
    val = m.group(1).strip()
    if not val:
        return False
    # Next-key false positive when the value line is empty in Unity YAML.
    if val.endswith(":"):
        return False
    next_key_prefixes = (
        "chess",
        "baseProperty",
        "plantTags",
        "fetterMemberId",
        "chessTileType",
        "chessPre",
        "chessSprite",
        "detectMode",
        "config",
        "num",
        "tier",
    )
    if any(val.startswith(p) for p in next_key_prefixes):
        return False
    if val.startswith('"'):
        inner = val.strip('"')
        return bool(inner.strip())
    if val in ("[]", "{}"):
        return False
    return True


def parse_chess_name(content, filename):
    """Parse chessName from Unity YAML without crossing lines (\\s would swallow next key)."""
    m = re.search(r'^\s*chessName:\s*"([^"]*)"\s*$', content, re.M)
    if m and m.group(1).strip():
        return decode_unicode_name(m.group(1))
    m = re.search(r"^\s*chessName:\s*([^\s\r\n]+)\s*$", content, re.M)
    if m:
        val = m.group(1).strip()
        if val and not val.endswith(":"):
            return decode_unicode_name(val)
    m = re.search(r'^\s*m_Name:\s*"([^"]+)"', content, re.M)
    if m and m.group(1).strip():
        return decode_unicode_name(m.group(1))
    return filename.replace(".asset", "")


def parse_property_creator(content, filename):
    def m(pat, default=""):
        r = re.search(pat, content, re.M)
        return r.group(1) if r else default

    name = parse_chess_name(content, filename)
    has_desc = field_nonempty(content, "chessDescription")
    has_short = field_nonempty(content, "chessShortDescription")
    has_effect = field_nonempty(content, "chessEffect")
    has_any_text = has_desc or has_short or has_effect
    has_full_text = has_desc and has_short and has_effect
    has_sprite = "chessSprite:" in content and not re.search(
        r"chessSprite:\s*\{fileID:\s*0\}", content
    )
    has_prefab = "chessPre:" in content and not re.search(
        r"chessPre:\s*\{fileID:\s*0\}", content
    )
    tag_block = re.search(r"plantTags:(.*?)(?:\n  fetterMemberId|\n  chessTileType)", content, re.S)
    tags = re.findall(r'-\s+"([^"]+)"', tag_block.group(1)) if tag_block else []
    member_id = m(r'fetterMemberId:\s*"([^"]*)"').strip()
    price = m(r"^\s+price:\s*(\d+)", "")
    hp = m(r"^\s+HpMax:\s*(\d+)", m(r"^\s+Hp:\s*(\d+)", ""))
    func = m(r"type:\s*\{class:\s*(\w+)")
    missing = []
    if not has_any_text:
        missing.append("文案全空")
    if not has_desc:
        missing.append("缺描述")
    if not has_short:
        missing.append("缺简介")
    if not has_effect:
        missing.append("缺效果")
    if not has_sprite:
        missing.append("缺图标")
    if not has_prefab:
        missing.append("缺Prefab")
    if not tags:
        missing.append("无plantTags")
    return {
        "file": filename,
        "name": name,
        "hasDescription": has_desc,
        "hasShortDescription": has_short,
        "hasEffect": has_effect,
        "hasAnyText": has_any_text,
        "hasFullText": has_full_text,
        "hasSprite": has_sprite,
        "hasPrefab": has_prefab,
        "tags": tags,
        "fetterMemberId": member_id,
        "price": price,
        "hp": hp,
        "plantFunction": func,
        "missing": missing,
    }


def parse_fetter(content, filename):
    name = filename.replace(".asset", "")
    m_name = re.search(r'fetterName:\s*"([^"]*)"', content)
    if m_name:
        name = decode_unicode_name(m_name.group(1))
    has_desc = field_nonempty(content, "fetterEffectDescription")
    # tier thresholds count
    tiers = re.findall(r"tierThresholds:", content)
    return {
        "file": filename,
        "name": name,
        "hasEffectDescription": has_desc,
        "missing": [] if has_desc else ["缺fetterEffectDescription"],
    }


def scan_dir(dir_path, parser):
    rows = []
    if not os.path.isdir(dir_path):
        return rows
    for fn in sorted(os.listdir(dir_path)):
        if not fn.endswith(".asset"):
            continue
        rows.append(parser(read_text(os.path.join(dir_path, fn)), fn))
    return rows


def summarize(label, rows):
    total = len(rows)
    is_fetter = rows and "hasEffectDescription" in rows[0]
    if is_fetter:
        no_text = [r for r in rows if not r.get("hasEffectDescription")]
        return {
            "label": label,
            "total": total,
            "noText": len(no_text),
            "partialText": 0,
            "noTextItems": [r["name"] for r in no_text],
            "partialItems": [],
        }

    no_text = [r for r in rows if not r.get("hasAnyText")]
    partial = [r for r in rows if r.get("hasAnyText") and not r.get("hasFullText")]
    no_desc_short = [
        r for r in rows if not r.get("hasDescription") and not r.get("hasShortDescription")
    ]
    full = [r for r in rows if r.get("hasFullText")]
    return {
        "label": label,
        "total": total,
        "noText": len(no_text),
        "partialText": len(partial),
        "noDescAndShort": len(no_desc_short),
        "fullTrio": len(full),
        "hasAnyText": total - len(no_text),
        "noTextItems": [r["name"] for r in no_text],
        "partialItems": [r["name"] for r in partial],
        "noDescAndShortItems": [r["name"] for r in no_desc_short],
        "hasAnyTextItems": [r["name"] for r in rows if r.get("hasAnyText")],
        "fullTrioItems": [r["name"] for r in full],
    }


def md_table(headers, rows):
    lines = ["| " + " | ".join(headers) + " |", "| " + " | ".join(["---"] * len(headers)) + " |"]
    for row in rows:
        lines.append("| " + " | ".join(str(c) for c in row) + " |")
    return "\n".join(lines)


def build_markdown(plants, enemies, fetters):
    ps = summarize("plants", plants)
    es = summarize("enemies", enemies)
    fs = summarize("fetters", fetters)

    plant_no_tags = [p["name"] for p in plants if "无plantTags" in p["missing"]]
    plant_no_icon = [p["name"] for p in plants if "缺图标" in p["missing"]]

    def fmt_names(names, limit=20):
        if not names:
            return "_无_"
        head = ", ".join(names[:limit])
        return head + ("…" if len(names) > limit else "")

    lines = [
        "# 内容配置缺口扫描（RG-CONTENT / RG-A01 / RG-C06 / RG-C07）",
        "",
        f"> 自动生成日期：{date.today().isoformat()}  ",
        f"> 脚本：`tools/audit_content_config.py`  ",
        f"> 原始数据：`tools/content_config_gaps.json`",
        "",
        "> **说明**：Unity 空字符串在 YAML 中为 `field:\\n`（非 `field: \"\"`）。脚本已过滤「下一行 key 被误判为值」的情况。",
        "",
        "## 摘要",
        "",
        md_table(
            ["类别", "总数", "三字段全空", "描述+简介都缺", "有部分文案", "三字段齐全"],
            [
                [
                    "植物 Player SO",
                    ps["total"],
                    ps["noText"],
                    ps.get("noDescAndShort", "-"),
                    ps["partialText"],
                    ps.get("fullTrio", "-"),
                ],
                [
                    "僵尸 Enemy SO",
                    es["total"],
                    es["noText"],
                    es.get("noDescAndShort", "-"),
                    es["partialText"],
                    es.get("fullTrio", "-"),
                ],
                [
                    "羁绊 Fetter SO",
                    fs["total"],
                    fs["noText"],
                    "-",
                    "-",
                    fs["total"] - fs["noText"],
                ],
            ],
        ),
        "",
        "字段：`chessDescription`（描述）、`chessShortDescription`（简介）、`chessEffect`（效果）",
        "",
        "### 植物其他缺口",
        "",
        f"- **无 plantTags**：{len(plant_no_tags)} 张 — {fmt_names(plant_no_tags, 15)}",
        f"- **缺 chessSprite 图标**：{len(plant_no_icon)} 张",
        "",
        "## 植物 · 已有任意文案（{} 张）".format(ps.get("hasAnyText", 0)),
        "",
        fmt_names(ps.get("hasAnyTextItems", [])),
        "",
        "## 植物 · 文案全空（{} 张）".format(ps["noText"]),
        "",
        fmt_names(ps["noTextItems"]),
        "",
        "## 植物 · 有部分文案（缺描述/简介/效果之一）",
        "",
    ]
    partial_plants = [p for p in plants if p["hasAnyText"] and not p.get("hasFullText")]
    if partial_plants:
        lines.append(
            md_table(
                ["名称", "描述", "简介", "效果", "缺口"],
                [
                    [
                        p["name"],
                        "Y" if p["hasDescription"] else "",
                        "Y" if p["hasShortDescription"] else "",
                        "Y" if p["hasEffect"] else "",
                        "; ".join(x for x in p["missing"] if x.startswith("缺")),
                    ]
                    for p in partial_plants
                ],
            )
        )
    else:
        lines.append("_无_")

    lines += [
        "",
        "## 僵尸 · 文案全空（{} 张）".format(es["noText"]),
        "",
        fmt_names(es["noTextItems"]) if es["noTextItems"] else "_无_",
        "",
        "## 僵尸 · 有部分文案",
        "",
    ]
    partial_enemies = [e for e in enemies if e["hasAnyText"]]
    if partial_enemies:
        lines.append(
            md_table(
                ["名称", "描述", "简介", "效果"],
                [
                    [
                        e["name"],
                        "Y" if e["hasDescription"] else "",
                        "Y" if e["hasShortDescription"] else "",
                        "Y" if e["hasEffect"] else "",
                    ]
                    for e in partial_enemies
                ],
            )
        )
    else:
        lines.append("_无_")

    lines += ["", "## 羁绊 · 缺 fetterEffectDescription", ""]
    f_missing = [f for f in fetters if not f["hasEffectDescription"]]
    if f_missing:
        lines.append(", ".join(f["name"] for f in f_missing))
    else:
        lines.append("_全部已填_")

    lines += [
        "",
        "## 羁绊 · 已填说明",
        "",
        md_table(
            ["名称", "已填"],
            [[f["name"], "Y" if f["hasEffectDescription"] else ""] for f in fetters],
        ),
        "",
        "## 建议填法（Unity Inspector）",
        "",
        "| 类型 | 路径 | 字段 |",
        "|------|------|------|",
        "| 植物 | `Assets/Resources/ChessData/Player/` | chessDescription / chessShortDescription / chessEffect |",
        "| 僵尸 | `Assets/Resources/ChessData/Enemy/` | 同上 |",
        "| 羁绊 | `Assets/SO/Fetter/` | fetterEffectDescription |",
        "",
        "改 SO 即可，**无需改脚本**；`PlantCreatorDetailHelper` / `FetterIcon` 已读取上述字段。",
        "",
    ]
    return "\n".join(lines)


def main():
    plants = scan_dir(PLAYER_DIR, parse_property_creator)
    enemies = scan_dir(ENEMY_DIR, parse_property_creator)
    fetters = scan_dir(FETTER_DIR, parse_fetter)

    payload = {
        "generated": date.today().isoformat(),
        "plants": plants,
        "enemies": enemies,
        "fetters": fetters,
        "summary": {
            "plants": summarize("plants", plants),
            "enemies": summarize("enemies", enemies),
            "fetters": summarize("fetters", fetters),
        },
    }

    with open(OUT_JSON, "w", encoding="utf-8") as f:
        json.dump(payload, f, ensure_ascii=False, indent=2)

    os.makedirs(os.path.dirname(OUT_MD), exist_ok=True)
    with open(OUT_MD, "w", encoding="utf-8") as f:
        f.write(build_markdown(plants, enemies, fetters))

    s = payload["summary"]
    print("=== Content config audit ===")
    print(
        f"Plants:  {len(plants)} total, all empty: {s['plants']['noText']}, "
        f"partial: {s['plants']['partialText']}, full trio: {s['plants'].get('fullTrio', 0)}"
    )
    print(
        f"Enemies: {len(enemies)} total, all empty: {s['enemies']['noText']}, "
        f"partial: {s['enemies']['partialText']}, full trio: {s['enemies'].get('fullTrio', 0)}"
    )
    print(f"Fetters: {len(fetters)} total, no desc: {s['fetters']['noText']}")
    print("Wrote", OUT_JSON)
    print("Wrote", OUT_MD)


if __name__ == "__main__":
    main()
