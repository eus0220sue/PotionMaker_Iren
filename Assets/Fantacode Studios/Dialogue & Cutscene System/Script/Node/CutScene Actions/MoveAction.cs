using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;


namespace FC_CutsceneSystem
{
    public class MoveAction : CutSceneAction
    {
        public GameObject objectToMove;
        public List<Vector3> movePath = new List<Vector3>() { new Vector3() };
        public Animator animator;
        public List<string> anmSourceList = new List<string>() { "" };
        public bool isPlayer;

        public int anmLayerIndex = 0;


        public Vector3 targetPos;
        public float moveSpeed = 2;
        public string anmSource;
        public bool playAnimation;
        public bool disableGravity = true;
        public MoveToSource targetPosUsing;
        public Transform targetPosObject;
        public bool usePlayerTransform;
        public float crossFadeTime = .3f;
        public MoveType moveUsing;
        public PlayAnimationUsing playAnimationUsing;
        public float stoppingDistance = 0;

        public AnimatorParameterType parameterType;

        public ObjectSource objectToMoveSource;
        public ObjectSource targetObjectSource;

        public List<AnimatorParameterType> parameterTypeV = new List<AnimatorParameterType>() { AnimatorParameterType.Bool };
        public List<bool> boolTriggerValueV = new List<bool>() { false };
        public List<int> intValueV = new List<int>() { 0 };
        public List<float> floatValueV = new List<float>() { 0 };

        public bool boolTriggerValueN;
        public int intValueN;
        public float floatValueN;

        public int intValueAfterAnimation;
        public float floatvalueAfterAnimation;

        bool foldout1 = false;
        bool foldout2 = false;


        public MoveAction(MoveAction move = null)
        {
            actionType = ActionType.Move;
#if UNITY_EDITOR
            if (move != null)
                EditorUtility.CopySerializedManagedFieldsOnly(move, this);
#endif
        }

        public override IEnumerator ExecuteAction()
        {
            if (objectToMoveSource == ObjectSource.UsePlayer)
                objectToMove = GameObject.FindGameObjectWithTag("Player");

            if (!ActionValidationWhilePlaying(objectToMove))
                yield break;

            if (objectToMove.gameObject.GetComponent<Animator>() != null)
                animator = objectToMove.gameObject.GetComponent<Animator>();
            else if(objectToMove.GetComponentInChildren<Animator>() != null)
                animator = objectToMove.GetComponentInChildren<Animator>();


            bool useGravityVal = false;
            float gravityScaleVal = 0;
            bool isKinematic = false;
            Rigidbody _rb = null;
            Rigidbody2D _rb2D = null;

            yield return new WaitForSecondsRealtime(.1f);

            // Disable Character Controller
            bool wasCharControllerEnabled = false;
            var charController = objectToMove.GetComponent<CharacterController>();
            if (charController != null)
            {
                wasCharControllerEnabled = charController.enabled;
                charController.enabled = false;
            }

            if (disableGravity)
            {
                if (objectToMove.GetComponent<Rigidbody>() != null)
                {
                    _rb = objectToMove.GetComponent<Rigidbody>();
                    useGravityVal = _rb.useGravity;
                    isKinematic = _rb.isKinematic;
                    _rb.isKinematic = true;
                    _rb.useGravity = false;
                }
                else if (objectToMove.GetComponent<Rigidbody2D>() != null)
                {
                    _rb2D = objectToMove.GetComponent<Rigidbody2D>();
                    gravityScaleVal = _rb2D.gravityScale;
                    isKinematic = _rb2D.isKinematic;

                    _rb2D.isKinematic = true;
                    _rb2D.gravityScale = 0;
                }
            }


            if (moveUsing == MoveType.UsingVectorPattern)
            {
                for (int i = 0; i < movePath.Count; i++)
                {
                    var targetPos = objectToMove.transform.position;

                    PlayAnimation(anmSourceList[i], boolTriggerValueV[i], intValueV[i], floatValueV[i], true,parameterTypeV[i]);

                    targetPos.x += movePath[i].x;
                    targetPos.y += movePath[i].y;
                    targetPos.z += movePath[i].z;
                    while (Vector3.Distance(targetPos, objectToMove.transform.position) > Mathf.Epsilon)
                    {
                        objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, targetPos, moveSpeed * Time.deltaTime);
                        yield return null;
                    }

                    PlayAnimation(anmSourceList[i], !boolTriggerValueV[i], intValueAfterAnimation, floatvalueAfterAnimation, false,parameterTypeV[i]);

                    objectToMove.transform.position = targetPos;
                }
            }
            else if (moveUsing == MoveType.UsingNavmesh || moveUsing == MoveType.UsingTransform)
            {
                if (targetObjectSource == ObjectSource.UsePlayer)
                    targetPosObject = GameObject.FindGameObjectWithTag("Player").transform;
                if (targetPosUsing == MoveToSource.Object && !ActionValidationWhilePlaying(targetPosObject))
                    yield break;
                Vector3 targetPos = targetPosUsing == MoveToSource.Position ? this.targetPos : targetPosObject.position;

                PlayAnimation(anmSource, boolTriggerValueN, intValueN, floatValueN, true,parameterType);

                if (moveUsing == MoveType.UsingNavmesh)
                {
                    var navMesh = objectToMove.GetComponent<NavMeshAgent>() != null ? objectToMove.GetComponent<NavMeshAgent>() : objectToMove.AddComponent<NavMeshAgent>();
                    navMesh.enabled = true;
                    navMesh.stoppingDistance = stoppingDistance;
                    navMesh.SetDestination(targetPos);
                    navMesh.speed = moveSpeed;
                    yield return new WaitForSecondsRealtime(.1f);
                    yield return new WaitUntil(() => !navMesh.hasPath || navMesh.remainingDistance <= stoppingDistance + 0.1f);
                    navMesh.enabled = false;
                }
                else if(moveUsing == MoveType.UsingTransform)
                {
                    while (Vector3.Distance(targetPos, objectToMove.transform.position) > Mathf.Epsilon)
                    {
                        objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, targetPos, moveSpeed * Time.deltaTime);
                        yield return null;
                    }
                }
                PlayAnimation(anmSource, !boolTriggerValueN, intValueAfterAnimation, floatvalueAfterAnimation, false,parameterType);

            }

            // Enable Character Controller
            if (charController != null)
                charController.enabled = wasCharControllerEnabled;

            if (disableGravity)
            {
                if (_rb != null)
                {
                    _rb.useGravity = useGravityVal;
                    _rb.isKinematic = isKinematic;
                }
                if (_rb2D != null)
                {
                    _rb2D.gravityScale = gravityScaleVal;
                    _rb2D.isKinematic = isKinematic;
                }
            }
        }
        
        void PlayAnimation(string anmSource,bool boolTriggerValueN, int intValueN, float floatValueN,bool beforePlay,AnimatorParameterType parameterType)
        {
            if (playAnimation && animator != null)
            {
                if (anmSource != "")
                {
                    if (beforePlay && playAnimationUsing == PlayAnimationUsing.AnimationName)
                        animator.CrossFadeInFixedTime(anmSource, crossFadeTime, anmLayerIndex);
                    else if (playAnimationUsing == PlayAnimationUsing.AnimatorParameter)
                    {
                        switch (parameterType) 
                        {
                            case AnimatorParameterType.Bool:
                                animator.SetBool(anmSource, boolTriggerValueN);
                                break;
                            case AnimatorParameterType.Int:
                                if (beforePlay) intValueAfterAnimation = animator.GetInteger(anmSource);
                                animator.SetInteger(anmSource, intValueN);
                                break;
                            case AnimatorParameterType.Float:
                                if (beforePlay) floatvalueAfterAnimation = animator.GetFloat(anmSource);
                                animator.SetFloat(anmSource, floatValueN);
                                break;
                            case AnimatorParameterType.Trigger:
                                animator.SetTrigger(anmSource);
                                break;
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as MoveAction;

            List<MoveAction> actions = new List<MoveAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as MoveAction).ToList();
                var firstNode = actions.First();
                node.objectToMove = (GameObject)GetFieldValue(actions.All(n => n.objectToMove == firstNode.objectToMove), null, firstNode.objectToMove);
                node.objectToMoveSource = (ObjectSource)GetFieldValue(actions.All(n => n.objectToMoveSource == firstNode.objectToMoveSource), ObjectSource.AssignObject, firstNode.objectToMoveSource);
                node.moveSpeed = (float)GetFieldValue(actions.All(n => n.moveSpeed == firstNode.moveSpeed), 0f, firstNode.moveSpeed);
                node.moveUsing = (MoveType)GetFieldValue(actions.All(n => n.moveUsing == firstNode.moveUsing), MoveType.UsingTransform, firstNode.moveUsing);
                node.targetPosUsing = (MoveToSource)GetFieldValue(actions.All(n => n.targetPosUsing == firstNode.targetPosUsing), MoveToSource.Object, firstNode.targetPosUsing);
                node.targetPos.x = (float)GetFieldValue(actions.All(n => n.targetPos.x == firstNode.targetPos.x), 0f, firstNode.targetPos.x);
                node.targetPos.y = (float)GetFieldValue(actions.All(n => n.targetPos.y == firstNode.targetPos.y), 0f, firstNode.targetPos.y);
                node.targetPos.z = (float)GetFieldValue(actions.All(n => n.targetPos.z == firstNode.targetPos.z), 0f, firstNode.targetPos.z);
                node.targetPosObject = (Transform)GetFieldValue(actions.All(n => n.targetPosObject == firstNode.targetPosObject), null, firstNode.targetPosObject);
                node.targetObjectSource = (ObjectSource)GetFieldValue(actions.All(n => n.targetObjectSource == firstNode.targetObjectSource), ObjectSource.AssignObject, firstNode.targetObjectSource);
                node.stoppingDistance = (float)GetFieldValue(actions.All(n => n.stoppingDistance == firstNode.stoppingDistance),0f, firstNode.stoppingDistance);
                node.playAnimation = (bool)GetFieldValue(actions.All(n => n.playAnimation == firstNode.playAnimation),false, firstNode.playAnimation);
                node.playAnimationUsing = (PlayAnimationUsing)GetFieldValue(actions.All(n => n.playAnimationUsing == firstNode.playAnimationUsing), PlayAnimationUsing.AnimationName, firstNode.playAnimationUsing);
                node.parameterType = (AnimatorParameterType)GetFieldValue(actions.All(n => n.parameterType == firstNode.parameterType), AnimatorParameterType.Bool, firstNode.parameterType);
                node.anmSource = (string)GetFieldValue(actions.All(n => n.anmSource == firstNode.anmSource), "--", firstNode.anmSource);
                node.boolTriggerValueN = (bool)GetFieldValue(actions.All(n => n.boolTriggerValueN == firstNode.boolTriggerValueN), false, firstNode.boolTriggerValueN);
                node.intValueN = (int)GetFieldValue(actions.All(n => n.intValueN == firstNode.intValueN), 0, firstNode.intValueN);
                node.floatValueN = (float)GetFieldValue(actions.All(n => n.floatValueN == firstNode.floatValueN), 0f, firstNode.floatValueN);
                node.anmSource = (string)GetFieldValue(actions.All(n => n.anmSource == firstNode.anmSource), "--", firstNode.anmSource);
                node.anmLayerIndex = (int)GetFieldValue(actions.All(n => n.anmLayerIndex == firstNode.anmLayerIndex), 0, firstNode.anmLayerIndex);
                node.crossFadeTime = (float)GetFieldValue(actions.All(n => n.crossFadeTime == firstNode.crossFadeTime), 0f, firstNode.crossFadeTime);
                node.disableGravity = (bool)GetFieldValue(actions.All(n => n.disableGravity == firstNode.disableGravity), false, firstNode.disableGravity);
                node.foldout2 = (bool)GetFieldValue(actions.All(n => n.foldout2 == firstNode.foldout2), false, firstNode.foldout2);


            }

            //Object To Move
            GUILayout.BeginHorizontal();
            GameObject objectToMove = null;
            if (node.objectToMoveSource == ObjectSource.AssignObject)
                objectToMove = (GameObject)EditorGUILayout.ObjectField("Object To Move", node.objectToMove, typeof(GameObject), true);
            else
            {
                GUILayout.Label("Object To Move");
                GUILayout.Label(new GUIContent("Player", "Player-tagged object"));
            }
            node.objectToMoveSource = (ObjectSource)EditorGUILayout.EnumPopup(node.objectToMoveSource, GUILayout.Width(20));
            if (objectToMove != node.objectToMove || objectToMoveSource != node.objectToMoveSource)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                {
                    n.objectToMove = objectToMove;
                    n.objectToMoveSource = objectToMoveSource;
                }
            }
            GUILayout.EndHorizontal();
            if (node.objectToMoveSource == ObjectSource.AssignObject)
                ValidationWarning(node.objectToMove, "Object is not assigned", graph);
            GUILayout.Space(5);



            var moveSpeed = EditorGUILayout.FloatField("Move Speed", node.moveSpeed);
            if (moveSpeed != node.moveSpeed)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.moveSpeed = moveSpeed;
            }
            GUILayout.Space(5);

            var moveUsing = (MoveType)EditorGUILayout.EnumPopup("Move Type", node.moveUsing);
            if (moveUsing != node.moveUsing)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.moveUsing = moveUsing;
            }
           
            //MOVE PATTERN
            if (node.moveUsing == MoveType.UsingVectorPattern)
            {
                if (selectedNodes == null)
                {
                    GUILayout.Space(5);
                    foldout1 = (bool)graph.SetField(foldout1, EditorGUILayout.Foldout(foldout1, "Move Pattern"));
                    if (foldout1)
                    {

                        GUILayout.Space(5);
                        for (int i = 0; i < node.movePath.Count; i++)
                        {

                            GUILayout.BeginHorizontal();
                            EditorGUI.indentLevel++;
                            node.movePath[i] = (Vector3)graph.SetField(node.movePath[i], EditorGUILayout.Vector3Field("", node.movePath[i]));
                            EditorGUI.indentLevel--;
                            if (playAnimation)
                            {
                                GUILayout.Space(5);
                                node.anmSourceList[i] = (string)graph.SetField(node.anmSourceList[i], EditorGUILayout.TextField(node.anmSourceList[i]));
                                CutsceneEditorManager.SetPlaceHolder(node.anmSourceList[i], node.playAnimationUsing + "..", Color.gray);
                                if (node.playAnimationUsing == PlayAnimationUsing.AnimatorParameter)
                                {
                                    node.parameterTypeV[i] = (AnimatorParameterType)graph.SetField(node.parameterTypeV[i], (AnimatorParameterType)EditorGUILayout.EnumPopup(node.parameterTypeV[i], GUILayout.Width(50)));
                                    switch (node.parameterTypeV[i])
                                    {
                                        case AnimatorParameterType.Bool:
                                            node.boolTriggerValueV[i] = (bool)graph.SetField(node.boolTriggerValueV[i], EditorGUILayout.Toggle("", node.boolTriggerValueV[i], GUILayout.Width(25)));
                                            break;
                                        case AnimatorParameterType.Int:
                                            node.intValueV[i] = (int)graph.SetField(node.intValueV[i], EditorGUILayout.IntField("", node.intValueV[i], GUILayout.Width(25)));
                                            break;
                                        case AnimatorParameterType.Float:
                                            node.floatValueV[i] = (float)graph.SetField(node.floatValueV[i], EditorGUILayout.FloatField("", node.floatValueV[i], GUILayout.Width(25)));
                                            break;
                                        case AnimatorParameterType.Trigger:
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            if (node.movePath.Count > 1)
                            {
                                if (GUILayout.Button("-", GUILayout.Width(20)))
                                {
                                    node.movePath.RemoveAt(i);
                                    node.anmSourceList.RemoveAt(i);
                                    node.parameterTypeV.RemoveAt(i);
                                    node.boolTriggerValueV.RemoveAt(i);
                                    node.intValueV.RemoveAt(i);
                                    node.floatValueV.RemoveAt(i);
                                }
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(5);

                            if (i == node.movePath.Count - 1)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(15);
                                if (GUILayout.Button("Add pattern", GUILayout.Width(80)))
                                {
                                    node.movePath.Add(new Vector3());
                                    node.anmSourceList.Add("");
                                    node.parameterTypeV.Add(AnimatorParameterType.Bool);
                                    node.boolTriggerValueV.Add(false);
                                    node.intValueV.Add(0);
                                    node.floatValueV.Add(0);
                                }
                                GUILayout.EndHorizontal();
                            }

                        }
                    }
                }
            }

            //NAVMESH AND TRANSFORM
            else if (node.moveUsing == MoveType.UsingNavmesh || node.moveUsing == MoveType.UsingTransform)
            {
                GUILayout.Space(5);
                var targetPosUsing = (MoveToSource)EditorGUILayout.EnumPopup("Move To", node.targetPosUsing);
                GUILayout.Space(5);
                Transform targetPosObject = null;
                ObjectSource targetObjectSource = ObjectSource.AssignObject;
                Vector3 targetPos = Vector3.zero;
                if (node.targetPosUsing == MoveToSource.Position)
                {
                    targetPos = EditorGUILayout.Vector3Field("Target Position", node.targetPos);
                }
                if (node.targetPosUsing == MoveToSource.Object)
                {
                    GUILayout.BeginHorizontal();
                    
                    if (node.targetObjectSource == ObjectSource.AssignObject)
                        targetPosObject = (Transform)EditorGUILayout.ObjectField("Target Object", node.targetPosObject, typeof(Transform), true);
                    else
                    {
                        GUILayout.Label("Target Object");
                        GUILayout.Label(new GUIContent("Player Transform", "Player-tagged object's Transform"));
                    }
                    targetObjectSource = (ObjectSource)EditorGUILayout.EnumPopup(node.targetObjectSource, GUILayout.Width(20));
                    
                    GUILayout.EndHorizontal();
                    if (node.targetObjectSource == ObjectSource.AssignObject)
                        ValidationWarning(node.targetPosObject, "Object is not assigned", graph);
                }
                if (targetPosObject != node.targetPosObject || targetObjectSource != node.targetObjectSource || targetPosUsing != node.targetPosUsing || targetPos != node.targetPos)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                    {
                        n.targetPosObject = targetPosObject;
                        n.targetObjectSource = targetObjectSource;
                        n.targetPosUsing = targetPosUsing;
                        n.targetPos = targetPos;
                    }
                }
            }

            //ADDITIONAL OPTIONS
            GUILayout.Space(5);
            var foldout2 = EditorGUILayout.Foldout(node.foldout2, "Additional");
            if (foldout2 != node.foldout2)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.foldout2 = foldout2;
            }
            if (node.foldout2)
            {
                EditorGUI.indentLevel++;
                if (node.moveUsing == MoveType.UsingNavmesh)
                {
                    GUILayout.Space(5);
                    var stoppingDistance = EditorGUILayout.FloatField("NavMesh Stopping Distance", node.stoppingDistance);
                    if (stoppingDistance != node.stoppingDistance)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.stoppingDistance = stoppingDistance;
                    }
                }
               
                GUILayout.Space(5);
                var playAnimation = EditorGUILayout.Toggle("Play Animation", node.playAnimation);
                if (playAnimation != node.playAnimation)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                        n.playAnimation = playAnimation;
                }

                if (node.playAnimation)
                {
                    GUILayout.Space(5);
                    var playAnimationUsing = (PlayAnimationUsing)EditorGUILayout.EnumPopup("Play Using", node.playAnimationUsing);
                    if (playAnimationUsing != node.playAnimationUsing)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.playAnimationUsing = playAnimationUsing;
                    }

                    if (node.moveUsing == MoveType.UsingNavmesh || node.moveUsing == MoveType.UsingTransform)
                    {
                        if (node.playAnimationUsing == PlayAnimationUsing.AnimatorParameter)
                        {
                            GUILayout.Space(5);
                            var parameterType = (AnimatorParameterType)EditorGUILayout.EnumPopup("Parameter Type", node.parameterType);
                            if (parameterType != node.parameterType)
                            {
                                UndoGraph(graph);
                                foreach (var n in actions)
                                    n.parameterType = parameterType;
                            }
                            GUILayout.Space(5);

                            GUILayout.BeginHorizontal();
                            var anmSource = EditorGUILayout.TextField(node.playAnimationUsing.ToString().CleanForUI(), node.anmSource);
                            if (anmSource != node.anmSource)
                            {
                                UndoGraph(graph);
                                foreach (var n in actions)
                                    n.anmSource = anmSource;
                            }

                            EditorGUI.indentLevel--;
                            GUILayout.Space(5);
                            switch (node.parameterType)
                            {
                                case AnimatorParameterType.Bool:
                                    var boolTriggerValueN = EditorGUILayout.Toggle(node.boolTriggerValueN, GUILayout.Width(25));
                                    if (boolTriggerValueN != node.boolTriggerValueN)
                                    {
                                        UndoGraph(graph);
                                        foreach (var n in actions)
                                            n.boolTriggerValueN = boolTriggerValueN;
                                    }
                                    break;
                                case AnimatorParameterType.Int:
                                    var intValueN = EditorGUILayout.IntField(node.intValueN, GUILayout.Width(25));
                                    if (intValueN != node.intValueN)
                                    {
                                        UndoGraph(graph);
                                        foreach (var n in actions)
                                            n.intValueN = intValueN;
                                    }
                                    break;
                                case AnimatorParameterType.Float:
                                    var floatValueN = EditorGUILayout.FloatField(node.floatValueN, GUILayout.Width(25));
                                    if (floatValueN != node.floatValueN)
                                    {
                                        UndoGraph(graph);
                                        foreach (var n in actions)
                                            n.floatValueN = floatValueN;
                                    }
                                    break;
                                case AnimatorParameterType.Trigger:
                                    break;
                                default:
                                    break;
                            }
                            EditorGUI.indentLevel++;
                            GUILayout.EndHorizontal();
                        }
                        else if (node.playAnimationUsing == PlayAnimationUsing.AnimationName)
                        {
                            GUILayout.Space(5);
                            var anmSource = EditorGUILayout.TextField(node.playAnimationUsing.ToString().CleanForUI(), node.anmSource);
                            if (anmSource != node.anmSource)
                            {
                                UndoGraph(graph);
                                foreach (var n in actions)
                                    n.anmSource = anmSource;
                            }
                            //CutsceneEditorManager.SetPlaceHolder(node.anmSource, "Enter " + node.playAnimationUsing.ToString().CleanForUI() + "..", Color.grey);
                        }
                    }
                    if (node.playAnimationUsing == PlayAnimationUsing.AnimationName)
                    {
                        GUILayout.Space(5);
                        var anmLayerIndex = EditorGUILayout.IntField("Animator Layer",node.anmLayerIndex);
                        if (anmLayerIndex != node.anmLayerIndex)
                        {
                            UndoGraph(graph);
                            foreach (var n in actions)
                                n.anmLayerIndex = anmLayerIndex;
                        }
                        GUILayout.Space(5);
                        var crossFadeTime = EditorGUILayout.FloatField("Cross Fade Time", node.crossFadeTime);
                        if (crossFadeTime != node.crossFadeTime)
                        {
                            UndoGraph(graph);
                            foreach (var n in actions)
                                n.crossFadeTime = crossFadeTime;
                        }
                    }
                }

                GUILayout.Space(5);
                var disableGravity = EditorGUILayout.Toggle("Disable Gravity", node.disableGravity);
                if (disableGravity != node.disableGravity)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                        n.disableGravity = disableGravity;
                }

                EditorGUI.indentLevel--;
            }
        }
        public override bool Validation()
        {
            return (objectToMove == null && objectToMoveSource == ObjectSource.AssignObject)
                     || (targetPosObject == null && targetObjectSource == ObjectSource.AssignObject && targetPosUsing == MoveToSource.Object); 
        }
#endif
    }
}