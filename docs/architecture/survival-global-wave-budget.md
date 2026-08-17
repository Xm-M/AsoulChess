# 生存出怪：全局波号预算

**日期**: 2026-08-14

## 规则
- 本轮第 `i` 波（0-based）的全局波号 = `totalWavesCleared + i + 1`
- 例：`wavesPerRound=10`、第 3 轮开始且已清 20 波 → 本轮用全局 21～30
- `maxZombieValue`、`PassesWavePoolFilter` / `waveLimit`、旗帜波（`wave % 10 == 0`）均用**全局波号**
- 单波价值上限：`WaveData_Endless.MaxZombieValuePerWave = 12500`（≈ 500 普僵 × 25）

## 为何后面轮次更难
预算公式随 `wave` 增大（如 `(wave-1)/3`），全局波号跨轮递增 → 后轮单波 value 更高，直到封顶 12500。  
出场池种类仍由 `selectionIndex` 的 `initialPoolSize + growth` 扩大。
