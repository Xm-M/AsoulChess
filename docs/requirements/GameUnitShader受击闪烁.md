# 功能需求卡片: GameUnitShader `_FlashAmount` 受击闪烁

## 基本信息
- **功能名称**: `Unlit/GameUnitShader` 增加 `_FlashAmount` 等属性驱动受击闪白
- **所属模块**: 渲染（`Assets/Reanim2UnityAnim/GameUnitShader.shader`）+ Chess（已有 `SetFloat("_FlashAmount", Time.time)`）
- **需求类型**: 功能扩展
- **提出日期**: 2026-03-30

## 功能描述
- **语义**: `_FlashAmount` 写入 **`Time.time`** 为受击时刻；在原图色上做**加法**叠亮：`rgb += _FlashColor.rgb * max(0, _FlashPeak - (_Time.y - _FlashAmount) * _FlashSpeed) * _FlashColor.a`（与 Shader Graph 一致）。
- **关闭**: `_FlashAmount < 0` 时不叠加（材质默认 `-1`）。
- **可调**: `_FlashPeak`、`_FlashSpeed`；`_FlashColor` 为 HDR，**A 控制整体叠加强度**（透明感/弱闪）。

## 验收标准
- [ ] 使用 `GameUnitShader_Mat` 的 Sprite 受击时可见短时变亮
- [ ] 未受击时无持续误闪

## 关联 Context
- `context/index.md` — Chess / Material 受击反馈
