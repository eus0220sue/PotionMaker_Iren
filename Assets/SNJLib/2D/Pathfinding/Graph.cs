using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace Pathfinding
{
    /// <summary>
    /// A* 길찾기와 함께 사용할 그래프 인터페이스
    /// </summary>
    public interface IGraph
    {
        /// <summary>
        /// 위치 컬렉션
        /// </summary>
        /// <param name="argV">위치</param>
        /// <returns></returns>
        IEnumerable<Vector3Int> Neighbors(Vector3Int argV);

        /// <summary>
        /// 코스트
        /// </summary>
        /// <param name="argA">좌표A</param>
        /// <param name="argB">좌표B</param>
        /// <returns>탐색 비용</returns>
        float Cost(Vector3Int argA, Vector3Int argB);
    }

    /// <summary>
    /// 4방향(위,아래,왼쪽,오른쪽) 이동 타일맵 기반의 그래프
    /// </summary>
    public class FourDirGraph : IGraph
    {
        /// <summary>
        /// 근접 타일맵 방향
        /// </summary>
        static readonly Vector3Int[] s_neighbors = {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        /// <summary>
        /// 타일맵
        /// </summary>
        Tilemap m_map;

        /// <summary>
        /// 모든 값이 정수인 축 정렬 경계 상자 (AABB 충돌)
        /// </summary>
        BoundsInt m_bounds;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="argMap">타일맵</param>
        public FourDirGraph(Tilemap argMap)
        {
            this.m_map = argMap;
            this.m_bounds = argMap.cellBounds;
        }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="argMap">타일맵</param>
        /// <param name="argBounds">경계상자</param>
        public FourDirGraph(Tilemap argMap, BoundsInt argBounds)
        {
            this.m_map = argMap;
            this.m_bounds = argBounds;
        }

        /// <summary>
        /// 위치를 기반으로 경계상자와 충돌맵 체크 후 다음 목표위치 반환
        /// </summary>
        /// <param name="argV">현재위치</param>
        /// <returns>다음 목표 위치</returns>
        public IEnumerable<Vector3Int> Neighbors(Vector3Int argV)
        {
            foreach (Vector3Int _dir in s_neighbors)
            {
                Vector3Int _next = argV + _dir;

                if (m_bounds.Contains(_next) && m_map.GetTile(_next) == null)
                {
                    yield return _next;
                }
            }
        }

        /// <summary>
        /// 코스트
        /// </summary>
        /// <param name="argA">좌표A</param>
        /// <param name="argB">좌표B</param>
        /// <returns></returns>
        public float Cost(Vector3Int argA, Vector3Int argB)
        {
            return Vector3Int.Distance(argA, argB);
        }
    }
}
