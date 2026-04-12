#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

namespace MobaGameplay.Animation
{
    /// <summary>Tester temporal – verifica los 11 estados del AnimatorController en Play Mode.</summary>
    public class AnimationStateTester : MonoBehaviour
    {
        private Animator anim;
        private int passed;
        private int failed;

        private void Start()
        {
            anim = GetComponentInChildren<Animator>();
            if (anim == null) { Debug.LogError("[AnimTester] No Animator"); return; }
            StartCoroutine(RunAllTests());
        }

        // ── Helpers ──────────────────────────────────────────────
        private string Clip(int layer)
        {
            var c = anim.GetCurrentAnimatorClipInfo(layer);
            return c.Length > 0 ? c[0].clip.name : "(empty)";
        }

        private bool HasClip(int layer, string name)
        {
            foreach (var c in anim.GetCurrentAnimatorClipInfo(layer))
                if (c.clip.name == name) return true;
            return false;
        }

        private void Assert(string state, bool ok, string expected, string actual)
        {
            if (ok) { passed++; Debug.Log($"[AnimTester] PASS [{state}] '{actual}'"); }
            else    { failed++; Debug.LogWarning($"[AnimTester] FAIL [{state}] expected='{expected}' got='{actual}'"); }
        }

        // ── Test sequence ─────────────────────────────────────────
        private IEnumerator RunAllTests()
        {
            Debug.Log("[AnimTester] ===== INICIANDO TESTS =====");
            var wait02 = new WaitForSeconds(0.2f);
            var wait01 = new WaitForSeconds(0.1f);
            var wait015 = new WaitForSeconds(0.15f);
            var wait04 = new WaitForSeconds(0.4f);
            var wait05 = new WaitForSeconds(0.5f);
            var wait12 = new WaitForSeconds(1.2f);
            var wait20 = new WaitForSeconds(2.0f);
            var wait30 = new WaitForSeconds(3.0f);

            yield return new WaitForSeconds(0.3f); // estabilizar

            // 1. Idle
            anim.SetFloat("Speed", 0f);
            yield return wait02;
            Assert("1_Idle", Clip(0) == "Idle", "Idle", Clip(0));

            // 2. Walk
            anim.SetFloat("Speed", 2.0f);
            yield return wait02;
            Assert("2_Walk", HasClip(0, "Walk_N"), "Walk_N", Clip(0));

            // 3. Run
            anim.SetFloat("Speed", 5.335f);
            yield return wait02;
            Assert("3_Run", HasClip(0, "Run_N"), "Run_N", Clip(0));
            anim.SetFloat("Speed", 0f);
            yield return wait02;

            // Desactivar CharacterAnimator para controlar Animator manualmente
            var ca = GetComponent<CharacterAnimator>();
            if (ca != null) ca.enabled = false;
            yield return null;

            // 4. JumpStart
            anim.SetBool("Grounded", false);
            anim.SetBool("Jump", true);
            yield return wait01;
            Assert("4_JumpStart", Clip(0) == "JumpStart", "JumpStart", Clip(0));

            // 5. InAir (esperar JumpStart exitTime 0.66 * 0.40s)
            anim.SetBool("Jump", false);
            yield return wait04;
            Assert("5_InAir", Clip(0) == "InAir", "InAir", Clip(0));

            // 6. JumpLand
            anim.SetBool("Grounded", true);
            yield return wait02;
            Assert("6_JumpLand", Clip(0) == "JumpLand", "JumpLand", Clip(0));
            yield return wait05; // esperar que JumpLand termine

            if (ca != null) ca.enabled = true;
            yield return wait02;

            // 7. Dash (Base Layer)
            anim.SetTrigger("Dash");
            yield return wait015;
            Assert("7_Dash", Clip(0) == "roll", "roll", Clip(0));
            yield return wait12; // esperar que termine

            // 8a. BasicAttack (Upper Body)
            anim.SetTrigger("Attack");
            yield return wait015;
            Assert("8_BasicAttack", Clip(1) == "punch", "punch", Clip(1));

            // 8b. Base Layer no debe cambiar durante BasicAttack
            Assert("8b_Base_Intact", Clip(0) == "Idle", "Idle", Clip(0));
            yield return wait20;

            // 9. Cast (Upper Body)
            anim.SetTrigger("Cast");
            yield return wait015;
            Assert("9_Cast", Clip(1) == "spell2H", "spell2H", Clip(1));
            yield return wait30;

            // 10. Hit (Upper Body)
            anim.SetTrigger("Hit");
            yield return wait015;
            Assert("10_Hit", Clip(1) == "spell2H", "spell2H", Clip(1));
            yield return wait30;

            // 11. Death (Base Layer — terminal)
            anim.SetTrigger("Death");
            yield return wait015;
            Assert("11_Death", Clip(0) == "death", "death", Clip(0));

            Debug.Log($"[AnimTester] ===== RESULTADO FINAL: {passed}/11 PASS | {failed} FAIL =====");
            Destroy(this);
        }
    }
}
#endif
