// using System;
// using System.Threading.Tasks;
// using DG.Tweening;
// using UnityEngine;
// using Utils.Common;

// namespace Utils.ScriptableObjects.Variables {
//     [CreateAssetMenu(menuName = "Scriptable Variables/Float Variable")]
//     public class FloatVariable : ScriptableVariable<float> {
//         [NonSerialized] private bool removalRunning;
        
//         public async Task TryRemoveModifierGraduallyAsync(ReferenceableVariable<float> modifier, float targetValue, float removeTime) {
//             if (removalRunning) return;
//             removalRunning = true;
//             await DOTween.To(
//                 () => modifier.Value, 
//                 x => modifier.Value = x, targetValue, 
//                 removeTime).AsyncWaitForCompletion();
//             TryRemoveModifier(modifier);
//             removalRunning = false;
//         }
//     }
// }
using System;
using System.Threading.Tasks;
using UnityEngine;
using Utils.Common;

namespace Utils.ScriptableObjects.Variables
{
    [CreateAssetMenu(menuName = "Scriptable Variables/Float Variable")]
    public class FloatVariable : ScriptableVariable<float>
    {
        [NonSerialized] private bool removalRunning;

        // Gradually moves modifier.Value to targetValue over removeTime (in seconds),
        // without using DOTween's AsyncWaitForCompletion.
        public async Task TryRemoveModifierGraduallyAsync(
            ReferenceableVariable<float> modifier,
            float targetValue,
            float removeTime)
        {
            if (removalRunning) return;
            removalRunning = true;

            float startValue = modifier.Value;
            float elapsed = 0f;

            // Simple manual "tween" using Time.deltaTime and Lerp
            while (elapsed < removeTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / removeTime);
                modifier.Value = Mathf.Lerp(startValue, targetValue, t);

                // Yield control so we don't block the main thread
                await Task.Yield();
            }

            modifier.Value = targetValue;

            TryRemoveModifier(modifier);
            removalRunning = false;
        }
    }
}
