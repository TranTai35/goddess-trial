using UnityEngine;

[System.Serializable]
public class EnemySpawnEntry
{
    [Tooltip("Kéo đúng prefab enemy muốn xuất hiện trong khu vực này vào đây.")]
    public GameObject prefab;

    [Min(1)]
    [Tooltip("Tổng số enemy loại này trong mỗi wave.")]
    public int amount = 1;
}
