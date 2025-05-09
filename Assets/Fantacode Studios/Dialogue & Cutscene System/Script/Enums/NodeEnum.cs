using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public enum FacingDirection { Up, Down, Left, Right }
    public enum NodeType { Dialog, Choice, SetFact, Cutscene, Start, TriggerEvent, Random }
    public enum ConnectionType { Override, Multiple }
    public enum PlayAnimationUsing { AnimationName, AnimatorParameter }
    public enum MoveToSource { Position, Object }
    public enum RotationSource { RotationValue, RotateTowardsObject, AddRotation }
    public enum MoveType { UsingTransform, UsingNavmesh, UsingVectorPattern }
    public enum AnimatorParameterType { Bool, Float, Int, Trigger }
    public enum ObjectSource { AssignObject, UsePlayer }
    public enum AnimatorType { UnityAnimator,CustomAnimator2D }
    public enum TransformSource { SetValues, SetFromObject}
    public enum Toggle { True, False }
    public enum Windows { Characters, Localization, Facts, Settings }

}
