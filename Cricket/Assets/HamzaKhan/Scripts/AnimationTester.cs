using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CricketBowlingAnimations
{
    public class AnimationTester : MonoBehaviour
    {

        // Settings
        [Header("Settings")]
        [SerializeField] private Animator animator;

        [SerializeField] private string animatorBowlingTriggerName;
        [SerializeField] private string animatorBowlingTypeName;

        [SerializeField] private BowlingAnimation bowlingAnimation;

        [SerializeField] private bool useRootMotion = true;

        /// <summary>
        /// Bowling Animation Enum has all the possible bowling types.
        /// </summary>
        public enum BowlingAnimation
        {
            LeftArmFastBowler,
            LeftArmMediumFastBowler,
            LeftArmOrthodoxSpinner,
            LeftArmWristSpinner,
            RightArmFastBowler,
            RightArmMediumFastBowler,
            RightArmLegSpinner,
            RightArmOffSpinner,
        }

        // original position storer.
        private Vector3 originalPosition;

        // Notes
        [Header("NOTE")]
        [TextArea]
        public string Note;


        // Awake
        private void Awake()
        {
            originalPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            // change root motion mode.
            animator.applyRootMotion = useRootMotion;

            // set the float in the animator correctly.
            animator.SetFloat(animatorBowlingTypeName, (int)bowlingAnimation + 1);

            // check if the bowler isnt already bowling.
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Bowling"))
            {
                // check if we get input for 'P'
                if (Input.GetKeyDown(KeyCode.P))
                {
                    // Reset the bowler to origin.
                    transform.position = originalPosition;

                    // set the trigger.
                    animator.SetTrigger(animatorBowlingTriggerName);
                }
            }
        }
    }
}
