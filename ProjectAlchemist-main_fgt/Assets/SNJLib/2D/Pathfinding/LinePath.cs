using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// 라인 경로는 선분의 경로를 나타냄
    /// </summary>
    [System.Serializable]
    public class LinePath
    {
        /// <summary>
        /// 벡터3 노드
        /// </summary>
        public Vector3[] m_nodes;

        /// <summary>
        /// 최대 거리
        /// </summary>
        public float m_maxDist { get; private set; }

        /// <summary>
        /// 거리
        /// </summary>
        [System.NonSerialized]
        public float[] m_distances;

        /// <summary>
        /// 인덱서 선언
        /// </summary>
        /// <param name="i">노드 인덱스 번호</param>
        /// <returns>벡터3</returns>
        public Vector3 this[int i]
        {
            get
            {
                return m_nodes[i];
            }

            set
            {
                m_nodes[i] = value;
            }
        }

        /// <summary>
        /// 노드 길이
        /// </summary>
        public int Length
        {
            get
            {
                return m_nodes.Length;
            }
        }

        /// <summary>
        /// 노드의 끝 벡터값
        /// </summary>
        public Vector3 EndNode
        {
            get
            {
                return m_nodes[Length - 1];
            }
        }

        /// <summary>
        /// 라인 패스
        /// </summary>
        /// <param name="argNodes">노드 배열</param>
        public LinePath(Vector3[] argNodes)
        {
            m_nodes = argNodes;

            CalcDistances();
        }

        /// <summary>
        /// 라인 패스
        /// </summary>
        /// <param name="argNodes">노드 리스트</param>
        public LinePath(List<Vector3> argNodes)
        {
            m_nodes = argNodes.ToArray();

            CalcDistances();
        }

        /// <summary>
        /// 경로상의 각 노드가 시작 노드에서 어느정도 거리에 있는지 확인
        /// </summary>
        public void CalcDistances()
        {
            m_distances = new float[Length];
            m_distances[0] = 0;

            for (var i = 0; i < Length - 1; i++)
            {
                m_distances[i + 1] = m_distances[i] + Vector3.Distance(m_nodes[i], m_nodes[i + 1]);
            }

            m_maxDist = m_distances[m_distances.Length - 1];
        }

        /// <summary>
        /// 씬에 경로를 표시
        /// </summary>
        public void Draw()
        {
            for (int i = 0; i < Length - 1; i++)
            {
                Debug.DrawLine(m_nodes[i], m_nodes[i + 1], Color.cyan, 0.0f, false);
            }
        }

        /// <summary>
        /// 지정 위치에 대한 가장 가까운 지점에 대한 매개 변수를 반환
        /// </summary>
        /// <param name="argPosition">지정위치</param>
        /// <returns>가장 가까운 지점의 매개 변수</returns>
        public float GetParam(Vector3 argPosition)
        {
            // 경로에 가장 가까운 라인에서 첫 번째 포인트 찾기
            float _closestDist = DistToSegment(argPosition, m_nodes[0], m_nodes[1]);
            int _closestSegment = 0;

            for (int i = 1; i < Length - 1; i++)
            {
                float _dist = DistToSegment(argPosition, m_nodes[i], m_nodes[i + 1]);

                if (_dist <= _closestDist)
                {
                    _closestDist = _dist;
                    _closestSegment = i;
                }
            }

            float _param = m_distances[_closestSegment] + GetParamForSegment(argPosition, m_nodes[_closestSegment], m_nodes[_closestSegment + 1]);

            return _param;
        }

        /// <summary>
        /// Given a param it gets the position on the path.
        /// </summary>
        public Vector3 GetPosition(float argParam, bool argPathLoop = false)
        {
            /* Make sure the param is not past the beginning or end of the path */
            if (argParam < 0)
            {
                argParam = (argPathLoop) ? argParam + m_maxDist : 0;
            }
            else if (argParam > m_maxDist)
            {
                argParam = (argPathLoop) ? argParam - m_maxDist : m_maxDist;
            }

            /* Find the first node that is farther than given param */
            int i = 0;
            for (; i < m_distances.Length; i++)
            {
                if (m_distances[i] > argParam)
                {
                    break;
                }
            }

            /* Convert it to the first node of the line segment that the param is in */
            if (i > m_distances.Length - 2)
            {
                i = m_distances.Length - 2;
            }
            else
            {
                i -= 1;
            }

            /* Get how far along the line segment the param is */
            float _t = (argParam - m_distances[i]) / Vector3.Distance(m_nodes[i], m_nodes[i + 1]);

            /* Get the position of the param */
            return Vector3.Lerp(m_nodes[i], m_nodes[i + 1], _t);
        }

        /// <summary>
        /// Gives the distance of a point to a line segment.
        /// </summary>
        /// <param name="p">p is the point</param>
        /// <param name="v">v is one point of the line segment</param>
        /// <param name="w">w is the other point of the line segment</param>
        static float DistToSegment(Vector3 p, Vector3 v, Vector3 w)
        {
            Vector3 vw = w - v;

            float l2 = Vector3.Dot(vw, vw);

            if (Mathf.Approximately(l2, 0))
            {
                return Vector3.Distance(p, v);
            }

            float t = Vector3.Dot(p - v, vw) / l2;

            if (t < 0)
            {
                return Vector3.Distance(p, v);
            }

            if (t > 1)
            {
                return Vector3.Distance(p, w);
            }

            Vector3 closestPoint = Vector3.Lerp(v, w, t);

            return Vector3.Distance(p, closestPoint);
        }

        /// <summary>
        /// Finds the param for the closest point on the segment vw given the point p.
        /// </summary>
        static float GetParamForSegment(Vector3 p, Vector3 v, Vector3 w)
        {
            Vector3 vw = w - v;

            float l2 = Vector3.Dot(vw, vw);

            if (Mathf.Approximately(l2, 0))
            {
                return 0;
            }

            float t = Vector3.Dot(p - v, vw) / l2;

            if (t < 0)
            {
                t = 0;
            }
            else if (t > 1)
            {
                t = 1;
            }

            return t * Mathf.Sqrt(l2);
        }

        public void RemoveNode(int i)
        {
            Vector3[] newNodes = new Vector3[Length - 1];

            int newNodesIndex = 0;
            for (int j = 0; j < newNodes.Length; j++)
            {
                if (j != i)
                {
                    newNodes[newNodesIndex] = m_nodes[j];
                    newNodesIndex++;
                }
            }

            m_nodes = newNodes;

            CalcDistances();
        }
    }
}