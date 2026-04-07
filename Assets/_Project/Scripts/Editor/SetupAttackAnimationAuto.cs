using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

[InitializeOnLoad]
public class SetupAttackAnimationAuto {
    static SetupAttackAnimationAuto() {
        EditorApplication.delayCall += RunSetup;
    }

    static void RunSetup() {
        if (SessionState.GetBool("AttackSetupDone", false)) return;
        SessionState.SetBool("AttackSetupDone", true);

        Debug.Log("Automated setup starting for Attack Animation...");

        try {
            // 1. Mask
            string maskPath = "Assets/_Project/UpperBodyMask.mask";
            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
            if (mask == null) {
                mask = new AvatarMask();
                for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++) {
                    mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, true);
                }
                mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Root, false);
                mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftLeg, false);
                mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightLeg, false);
                AssetDatabase.CreateAsset(mask, maskPath);
            }

            // 2. Controller
            string controllerPath = "Assets/StarterAssets/ThirdPersonController/Character/Animations/StarterAssetsThirdPerson.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            
            if (controller == null) {
                Debug.LogError("Controller not found at: " + controllerPath);
                return;
            }

            if (!controller.parameters.Any(p => p.name == "Attack")) {
                controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            }

            int layerIndex = -1;
            for(int i=0; i<controller.layers.Length; i++) {
                if (controller.layers[i].name == "UpperBody") layerIndex = i;
            }

            if (layerIndex == -1) {
                AnimatorStateMachine sm = new AnimatorStateMachine {
                    name = "UpperBody",
                    hideFlags = HideFlags.HideInHierarchy
                };
                AssetDatabase.AddObjectToAsset(sm, controller);

                AnimatorControllerLayer layer = new AnimatorControllerLayer {
                    name = "UpperBody",
                    defaultWeight = 1f,
                    avatarMask = mask,
                    stateMachine = sm,
                    blendingMode = AnimatorLayerBlendingMode.Override
                };
                controller.AddLayer(layer);
                layerIndex = controller.layers.Length - 1;
            } else {
                // Update existing
                AnimatorControllerLayer[] layers = controller.layers;
                layers[layerIndex].defaultWeight = 1f;
                layers[layerIndex].avatarMask = mask;
                layers[layerIndex].blendingMode = AnimatorLayerBlendingMode.Override;
                controller.layers = layers;
            }

            AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
            
            // Clean states if needed or get them
            AnimatorState idleState = stateMachine.states.FirstOrDefault(s => s.state.name == "Idle/Movement").state;
            if (idleState == null) idleState = stateMachine.AddState("Idle/Movement");
            stateMachine.defaultState = idleState;

            string fbxPath = "Assets/_Project/Hook Punch.fbx";
            AnimationClip punchClip = null;
            
            if (System.IO.File.Exists(Application.dataPath + "/../" + fbxPath)) {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
                foreach(var asset in assets) {
                    if (asset is AnimationClip && !asset.name.Contains("__preview__") && !asset.name.StartsWith("Take ")) {
                        punchClip = asset as AnimationClip;
                        break;
                    }
                }
            }

            if (punchClip == null) {
                Debug.LogWarning("Could not find AnimationClip in " + fbxPath + ". Attack animation not auto-configured.");
                return;
            }

            AnimatorState attackState = stateMachine.states.FirstOrDefault(s => s.state.name == "Attack").state;
            if (attackState == null) attackState = stateMachine.AddState("Attack");
            attackState.motion = punchClip;

            // Transitions
            if (!idleState.transitions.Any(t => t.destinationState == attackState)) {
                AnimatorStateTransition toAttack = idleState.AddTransition(attackState);
                toAttack.hasExitTime = false;
                toAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            }

            if (!attackState.transitions.Any(t => t.destinationState == idleState)) {
                AnimatorStateTransition toIdle = attackState.AddTransition(idleState);
                toIdle.hasExitTime = true;
                toIdle.exitTime = 0.85f;
                toIdle.hasFixedDuration = true;
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=green>Attack Animation Auto-Setup Complete!</color> You can now test Left Click.");
        } catch (System.Exception e) {
            Debug.LogError("Auto-setup failed: " + e.Message);
        }
    }
}
