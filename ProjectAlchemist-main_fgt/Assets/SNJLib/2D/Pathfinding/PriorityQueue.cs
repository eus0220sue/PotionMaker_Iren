using System.Collections.Generic;

namespace Pathfinding
{
    /// <summary>
    /// 우선순위 대기열
    /// </summary>
    /// <typeparam name="T">템플릿</typeparam>
    public class PriorityQueue<T>
    {
        private List<Tuple<T>> m_list = new List<Tuple<T>>();

        public int Count
        {
            get
            {
                return m_list.Count;
            }
        }

        public void Enqueue(T item, float priority)
        {
            m_list.Add(new Tuple<T>(item, priority));
        }

        public T Dequeue()
        {
            T _result = default(T);

            if (m_list.Count > 0)
            {
                int index = 0;

                for (int i = 0; i < m_list.Count; i++)
                {
                    if (m_list[i].m_priority < m_list[index].m_priority)
                    {
                        index = i;
                    }
                }

                _result = m_list[index].m_item;
                m_list.RemoveAt(index);
            }

            return _result;
        }
    }

    struct Tuple<T>
    {
        public T m_item;
        public float m_priority;

        public Tuple(T item, float priority)
        {
            this.m_item = item;
            this.m_priority = priority;
        }
    }
}