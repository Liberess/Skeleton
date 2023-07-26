using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Utility
{
    /// <summary>
    /// center를 중심으로 distance만큼의 범위로
    /// areaMask에 포함되는 random한 좌표를 반환한다.
    /// </summary>
    public static Vector3 GetRandPointOnNavMesh(Vector3 center, float distance, int areaMask)
    {
        Vector3 randPos = Vector3.zero;
        NavMeshHit hit;

        for (int i = 0; i < 30; i++)
        {
            randPos = Random.insideUnitSphere * distance + center;

            if (NavMesh.SamplePosition(randPos, out hit, distance, areaMask))
                return hit.position;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 평균(mean)과 표준편차(standard)를 통해
    /// 정규분포 난수를 생성한다.
    /// </summary>
    public static float GetRandNormalDistribution(float mean, float standard)
    {
        var x1 = Random.Range(0f, 1f);
        var x2 = Random.Range(0f, 1f);
        return mean + standard * (Mathf.Sqrt(-2.0f * Mathf.Log(x1)) * Mathf.Sin(2.0f * Mathf.PI * x2));
    }

    /// <summary>
    /// 임의의 확률을 선택한다.
    /// ex) bool epicItem = GCR(0.001) → 1/1000의 확률로 크리티컬이 뜬다.
    /// </summary>
    public static bool GetChanceResult(float chance)
    {
        if (chance < 0.0000001f)
            chance = 0.0000001f;

        bool success = false;
        int randAccuracy = 10000000; // 천만. 천만분의 chance의 확률이다.
        float randHitRange = chance * randAccuracy;

        int rand = Random.Range(1, randAccuracy + 1);
        if (rand <= randHitRange)
            success = true;

        return success;
    }

    /// <summary>
    /// 임의의 퍼센트 확률을 선택한다.
    /// ex) bool critical = GPCR(30) → 30% 확률로 크리티컬이 뜬다.
    /// </summary>
    public static bool GetPercentageChanceResult(float perChance)
    {
        if (perChance < 0.0000001f)
            perChance = 0.0000001f;

        perChance = perChance / 100;

        bool success = false;
        int randAccuracy = 10000000; // 천만. 천만분의 chance의 확률이다.
        float randHitRange = perChance * randAccuracy;

        int rand = Random.Range(1, randAccuracy + 1);
        if (rand <= randHitRange)
            success = true;

        return success;
    }

    /// <summary>
    /// list에서 가장 가까운 거리에 있는 요소를 반환한다.
    /// </summary>
    public static GameObject GetNearestObjectByList(List<GameObject> list, Vector3 pos)
    {
        float minDistance = 1000.0f;
        GameObject tempObj = null;

        foreach (var obj in list)
        {
            float tempDistance = Vector3.Distance(
                pos, obj.transform.position);

            if (tempDistance <= minDistance)
            {
                tempObj = obj;
                minDistance = tempDistance;
            }
        }

        return tempObj;
    }
  
    /// <summary>
    /// target 위치에서 임의의 tag를 가진 가장 가까운 타겟을 반환한다.
    /// </summary>
    /// <returns></returns>
    public static GameObject FindNearestObjectByTag(Transform owner, string tag)
    {
        var objs = GameObject.FindGameObjectsWithTag(tag).ToList();

        var nearestObj = objs.OrderBy(obj =>
        {
            return Vector3.Distance(owner.position, obj.transform.position);
        }).FirstOrDefault();

        return nearestObj;
    }
    
    /// <summary>
    /// target이 메인 카메라 안에 있는지 확인한다.
    /// </summary>
    public static bool IsExistObjectInCamera(Transform target)
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(target.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1
                        && screenPoint.y > 0 && screenPoint.y < 1;

        return onScreen;
    }
}