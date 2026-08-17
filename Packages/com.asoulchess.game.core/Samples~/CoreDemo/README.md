# Core Demo Sample

1. In Unity: **Window → Package Manager** → select **AsoulChess Game Core** (In Project).
2. **Samples → Core Demo → Import**.
3. Create an empty scene, add an empty GameObject, attach `CoreDemoRunner`.
4. Enter Play Mode and press **Space**.

Expected Console flow:

```
ping received → schedule spawn
pooled object spawned → schedule release
released back to pool
```

AVZ gameplay code is **not** switched to Core in this release.
