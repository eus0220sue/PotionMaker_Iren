using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System;

namespace Pathfinding
{
    /// <summary>
    /// A* 길찾기
    /// </remarks>
    public static class AStar
    {
        /// <summary>
        /// 월드 좌표계를 사용하여 타일맵에서 이동 경로 취득
        /// </summary>
        /// <param name="argObsMap">충돌 타일맵</param>
        /// <param name="argNowPos">현재위치</param>
        /// <param name="argTargetPos">목표위치</param>
        /// <returns>이동경로(0: 현재위치)</returns>
        public static List<Vector3> FindPath(Tilemap argObsMap, Vector3 argNowPos, Vector3 argTargetPos)
        {
            List<Vector3> _result = null;
            List<Vector3Int> _path = FindPath(argObsMap, argObsMap.WorldToCell(argNowPos), argObsMap.WorldToCell(argTargetPos));

            if(_path != null)
            {
                _result = new List<Vector3>(_path.Capacity);

                foreach (Vector3Int _v in _path)
                {
                    Vector3Int _pos = _v;
                    _pos.z = 0;
                    // 그리드 셀의 중심 좌표를 월드 좌표로 변환하여 추가
                    _result.Add(argObsMap.GetCellCenterWorld(_v));
                }
            }

            return _result;
        }

        /// <summary>
        /// 셀 좌표를 사용하여 타일맵에서 이동 경로 취득
        /// </summary>
        /// <param name="argObsMap">충돌 타일맵</param>
        /// <param name="argNowPos">현재위치</param>
        /// <param name="argTargetPos">목표위치</param>
        /// <returns>이동경로 리스트(0: 현재위치)</returns>
        public static List<Vector3Int> FindPath(Tilemap argObsMap, Vector3Int argNowPos, Vector3Int argTargetPos)
        {
            return FindPath(new FourDirGraph(argObsMap), argNowPos, argTargetPos, Vector3Int.Distance);
        }

        /// <summary>
        /// 셀 좌표를 사용하여 타일맵에서 이동 경로 취득
        /// </summary>
        /// <param name="argGraph">그래프 인터페이스</param>
        /// <param name="argNowPos">현재위치</param>
        /// <param name="argTargetPos">목표위치</param>
        /// <param name="argHeuristic">의사결정</param>
        /// <returns>이동경로 리스트(0: 현재위치)</returns>
        public static List<Vector3Int> FindPath(IGraph argGraph, Vector3Int argNowPos, Vector3Int argTargetPos, Func<Vector3Int, Vector3Int, float> argHeuristic)
        {
            // 우선순위 큐
            PriorityQueue<Vector3Int> _open = new PriorityQueue<Vector3Int>();
            _open.Enqueue(argNowPos, 0);

            // 패스 정보 딕셔너리
            Dictionary<Vector3Int, Vector3Int> _cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            _cameFrom[argNowPos] = argNowPos;

            // 비용(코스트) 딕셔너리
            Dictionary<Vector3Int, float> _costSoFar = new Dictionary<Vector3Int, float>();
            _costSoFar[argNowPos] = 0;

            while (_open.Count > 0)
            {
                // 큐에서 하나를 꺼냄
                Vector3Int _current = _open.Dequeue();

                // 꺼낸 위치가 타겟위치와 같다면 종료
                if (_current == argTargetPos)
                {
                    break;
                }

                foreach (Vector3Int _next in argGraph.Neighbors(_current))
                {
                    // 다음 목표 위치의 비용 계산
                    float _newCost = _costSoFar[_current] + argGraph.Cost(_current, _next);

                    // 다음 목표 비용이 없거나 새로운 비용이 원래 존재 했던 비용보다 적다면
                    if (!_costSoFar.ContainsKey(_next) || _newCost < _costSoFar[_next])
                    {
                        _costSoFar[_next] = _newCost;
                        // 비용과 의사결정 값을 더해 우선순위 값 결정
                        float _priority = _newCost + argHeuristic(_next, argTargetPos);
                        // 큐에 우선순위 추가
                        _open.Enqueue(_next, _priority);
                        // 패스 정보 딕셔너리에 위치 정보 추가
                        _cameFrom[_next] = _current;
                    }
                }
            }

            // 반환할 패스 리스트
            List<Vector3Int> _path = null;

            // 패스 딕셔너리에 타겟 위치의 키가 존재할 경우에만 반환 리스트 작성
            if (_cameFrom.ContainsKey(argTargetPos))
            {
                _path = new List<Vector3Int>();

                Vector3Int _v = argTargetPos;

                // 딕셔너리의 값을 리스트에 넣음
                while(_v != argNowPos)
                {
                    _path.Add(_v);
                    _v = _cameFrom[_v];
                }

                // 현재 위치의 추가
                _path.Add(argNowPos);
                // 리스트를 역방향으로 전환
                _path.Reverse();
            }

            return _path;
        }
    }
}