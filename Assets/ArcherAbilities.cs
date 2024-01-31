using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewData", menuName = "Scriptable Objects/SimpleData")]
public class ArcherAbilities : ScriptableObject
{
    [SerializeField] ParticleSystem massArrow;
}
