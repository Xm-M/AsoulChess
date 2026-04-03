"""
Detect sprite frames in iceman.png by alpha gaps between columns/rows.
Writes iceman_sprites.json for Unity: Tools -> Sprite Sheet from JSON (select texture + this JSON).
Use default Pivot (1,0) bottom-right for side-view foot anchor; change in tool if needed.

Unity rect: origin bottom-left; PIL y is top-left, so y_unity = H - y_top - height
"""
from __future__ import annotations

import json
from pathlib import Path

from PIL import Image
import numpy as np

# --- tuning ---
ROW_CONTENT_THRESH = 500  # row sum alpha threshold for "has pixels"
COL_GAP_THRESH = 80  # column sum in band below this = gap between frames
MIN_FRAME_W = 2
MIN_FRAME_H = 2
MIN_GAP_WIDTH = 1  # min consecutive gap columns to split


def pil_to_unity_rect(x: int, y_top: int, w: int, h: int, img_h: int) -> dict:
    """PIL top-left origin -> Unity texture space bottom-left origin."""
    y_bottom = img_h - y_top - h
    return {"x": x, "y": y_bottom, "width": w, "height": h}


def split_band_vertical(a: np.ndarray, y0: int, y1: int) -> list[tuple[int, int, int, int]]:
    """Return list of (x,y_top,w,h) in PIL coords for frames in horizontal strip y0..y1."""
    sub = a[y0 : y1 + 1, :]
    col_sum = sub.sum(axis=0)
    h, w = sub.shape
    # gap columns
    is_gap = col_sum < COL_GAP_THRESH
    frames = []
    x = 0
    while x < w:
        # skip gap
        while x < w and is_gap[x]:
            x += 1
        if x >= w:
            break
        x0 = x
        while x < w and not is_gap[x]:
            x += 1
        x1 = x - 1
        fw = x1 - x0 + 1
        if fw < MIN_FRAME_W:
            continue
        # crop vertical to tight bounds inside this x-range
        patch = sub[:, x0 : x1 + 1]
        row_sum = patch.sum(axis=1)
        ry = np.where(row_sum > ROW_CONTENT_THRESH)[0]
        if len(ry) == 0:
            continue
        yt0, yt1 = ry.min(), ry.max()
        fh = yt1 - yt0 + 1
        if fh < MIN_FRAME_H:
            continue
        frames.append((x0 + 0, y0 + yt0, fw, fh))
    return frames


def main():
    root = Path(__file__).resolve().parent
    png = root / "iceman.png"
    img = Image.open(png).convert("RGBA")
    a = np.array(img)[:, :, 3]
    H, W = a.shape
    row_sum = a.sum(axis=1)

    # horizontal bands (row groups with visible pixels)
    in_band = row_sum > ROW_CONTENT_THRESH
    bands = []
    start = None
    for y in range(H):
        if in_band[y] and start is None:
            start = y
        elif not in_band[y] and start is not None:
            bands.append((start, y - 1))
            start = None
    if start is not None:
        bands.append((start, H - 1))

    all_frames: list[dict] = []
    idx = 0
    # skip very thin bands (text labels ~8px)
    for y0, y1 in bands:
        bh = y1 - y0 + 1
        if bh < 12:
            continue
        rects = split_band_vertical(a, y0, y1)
        for (x, y_top, fw, fh) in rects:
            ur = pil_to_unity_rect(int(x), int(y_top), int(fw), int(fh), int(H))
            name = f"iceman_{idx}"
            all_frames.append(
                {
                    "name": name,
                    "rect": {k: int(v) for k, v in ur.items()},
                    "pil": {"x": int(x), "y": int(y_top), "w": int(fw), "h": int(fh)},
                }
            )
            idx += 1

    out_json = root / "iceman_sprites.json"
    out_json.write_text(json.dumps(all_frames, indent=2), encoding="utf-8")
    print("Wrote", out_json, "frames:", len(all_frames))

    # also print summary for Unity meta manual paste count
    print("Sample rects (Unity space):", all_frames[:3])


if __name__ == "__main__":
    main()
