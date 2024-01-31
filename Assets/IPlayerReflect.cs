using System.Collections;
using UnityEngine;

public interface IPlayerReflect
{
    IEnumerator CanReflect(bool allow, GameObject target);
}
