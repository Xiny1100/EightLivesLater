using UnityEngine;

namespace Seagull.Interior_I1.Inspector
{
    public class MaterialHider : MonoBehaviour
    {
        [SerializeField] private Material[] hiddenMaterials;
        [SerializeField] private bool hideOnSelect = true;

        private Renderer[] renderers;
        private Material[][] originalMaterials;

        private void Start()
        {
            cacheOriginalMaterials();
        }

        private void cacheOriginalMaterials()
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalMaterials = new Material[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].materials;
            }
        }

        public void update(bool isSelected)
        {
            if (renderers == null || originalMaterials == null)
            {
                cacheOriginalMaterials();
            }

            if (hiddenMaterials == null || hiddenMaterials.Length == 0)
            {
                return;
            }

            bool shouldHide = hideOnSelect && isSelected;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (shouldHide)
                {
                    // Apply hidden materials
                    renderers[i].materials = hiddenMaterials;
                }
                else
                {
                    // Restore original materials
                    if (i < originalMaterials.Length)
                    {
                        renderers[i].materials = originalMaterials[i];
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Restore materials when destroyed
            if (!hideOnSelect && renderers != null && originalMaterials != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (i < originalMaterials.Length)
                    {
                        renderers[i].materials = originalMaterials[i];
                    }
                }
            }
        }
    }
}