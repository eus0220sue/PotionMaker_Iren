using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FC_CutsceneSystem
{

    [Serializable]
    public class Port
    {

        public List<PortConnection> connections = new List<PortConnection>();
        [NonSerialized] NodeBase parentNode;
        public ConnectionType connectionType;

        [HideInInspector]
        public string parentNodeId;
        [HideInInspector]
        public CutsceneGraph graph;

        [HideInInspector]
        [SerializeField] Rect rect;
        [HideInInspector]
        [SerializeField] string id;

        public Rect Rect
        {
            get { return rect; }
            set { rect = value; }
        }

        public string ID { get { return id; } }
        public NodeBase Node
        {
            get
            {
                if (parentNode != null)
                    return parentNode;

                parentNode = graph.nodes.FirstOrDefault(n => n.nodeID == parentNodeId);
                return parentNode;
            }
        }
#if UNITY_EDITOR
        public Port(NodeBase parentNode)
        {
            if (id == null)
                id = Guid.NewGuid().ToString();

            this.parentNode = parentNode;
            parentNodeId = parentNode.nodeID;

            graph = parentNode.parentGraph;
        }

        public void CreateConnection(Port portToAdd)
        {

            if (connectionType == ConnectionType.Override)
            {
                var oldConnectedPort = connections.FirstOrDefault();
                if (oldConnectedPort != null)
                    RemoveConnection(oldConnectedPort.Port);
            }
            if (portToAdd.connectionType == ConnectionType.Override)
            {
                var oldConnectedPort = portToAdd.connections.FirstOrDefault();
                if (oldConnectedPort != null)
                    portToAdd.RemoveConnection(oldConnectedPort.Port);
            }

            AddConnection(portToAdd);
            
        }

        public void RemoveConnection(Port portToRemove)
        {

            Undo.RegisterCompleteObjectUndo(graph, "Remove Connection");
            connections.RemoveAll(c => c.id == portToRemove.id);
            portToRemove.connections.RemoveAll(c => c.id == this.ID);
        }
        public void AddConnection(Port portToAdd)
        {
            Undo.RegisterFullObjectHierarchyUndo(graph, "Add Connection");
            connections.Add(new PortConnection(portToAdd));
            portToAdd.connections.Add(new PortConnection(this));

        }

        //remove connected ports when a node is deleted 
        public void RemoveconnectedPorts()
        {
            foreach (var p in connections)
            {
                Undo.RegisterCompleteObjectUndo(graph, "Remove Connections");
                p.Port.connections.RemoveAll(n => n.id == this.ID);
            }
        }
#endif
    }
}