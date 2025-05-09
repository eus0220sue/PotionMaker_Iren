using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace FC_CutsceneSystem
{
    [Serializable]
    public class PortConnection
    {
        [HideInInspector]
        [SerializeField] CutsceneGraph graph;
        [HideInInspector]
        public string id;
        [HideInInspector]
        public string nodeId;
        [HideInInspector]


        [NonSerialized] Port port;
        [NonSerialized] NodeBase node;

        [SerializeReference]
        public List<ConditionBase> conditions = new List<ConditionBase>();


        public Port Port { get { return port != null ? port : port = GetPort(); } }

        public NodeBase Node { get { return node != null ? node : node = graph.nodes.FirstOrDefault(n => n.nodeID == nodeId); } }
        public PortConnection(Port port)
        {
            this.port = port;
            node = port.Node;

            id = port.ID;
            nodeId = node.nodeID;
            graph = node.parentGraph;
        }
        private Port GetPort()
        {
            if (Node == null || string.IsNullOrEmpty(id))
                return null;
            return Node.GetPort(id);
        }
    }
}