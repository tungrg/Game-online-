using UnityEngine;

public class EnemyScoreValue : MonoBehaviour
{
    [SerializeField] public int killScore;

    public int KillScore => Mathf.Max(0, killScore);
}
