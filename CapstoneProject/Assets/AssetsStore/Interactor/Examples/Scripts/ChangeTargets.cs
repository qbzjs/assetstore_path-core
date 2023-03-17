using UnityEngine;

namespace razz
{
    public class ChangeTargets : MonoBehaviour
    {//Changes InteractorObject's targets
        public GameObject[] targets = new GameObject[0];
        public InteractorObject interactorObject;
        public int selected = -1;

        private InteractorTarget[][] newChildTargets;

        private void Start()
        {
            if (!interactorObject) return;

            int count = targets.Length;
            newChildTargets = new InteractorTarget[count][];
            if (count != 0)
            {
                for (int i = 0; i < count; i++)
                {
                    newChildTargets[i] = targets[i].GetComponentsInChildren<InteractorTarget>();
                }
            }
            ChangeTarget();
        }

        public void ChangeTarget()
        {//Gets called at the end of interaction by active target
            if (!interactorObject) return;
            if (targets.Length == 0) return;

            int count = targets.Length;
            selected++;
            if (selected >= count) selected = 0;
            if (newChildTargets[selected].Length == 0)
            {
                Debug.Log("Random target has no InteractorTarget: " + selected);
                return;
            }

            interactorObject.otherTargetsRoot = targets[selected];
            interactorObject.childTargets = newChildTargets[selected];
        }
    }
}
