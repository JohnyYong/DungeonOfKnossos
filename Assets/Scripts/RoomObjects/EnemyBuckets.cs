using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBuckets", menuName = "ScriptableObjects/EnemyBucket", order = 1)]
public class EnemyBuckets : ScriptableObject
{
    public List<GameObject> trivialEnemies;
    public List<GameObject> easyEnemies;
    public List<GameObject> mediumEnemies;
    public List<GameObject> hardEnemies;
}