using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace demo
{
    public class TerrainSwitcher : MonoBehaviour
    {
        public Text terrainNameText;
        public Text terrainDescText;
        public List<TerrainDesc> terrainsToSwitch;
        public KeyCode switchKey = KeyCode.C;
        public int activeTerrainIndex;

        private void OnEnable()
        {
            SwitchTerrain();
        }

        private void FixedUpdate()
        {
            if (DebouncedInput.GetKey(switchKey))
            {
                activeTerrainIndex = activeTerrainIndex >= terrainsToSwitch.Count - 1 
                    ? 0 
                    : activeTerrainIndex + 1;
                SwitchTerrain();
            }
        }

        private void SwitchTerrain()
        {
            for (var i = 0; i < terrainsToSwitch.Count; i++)
            {
                var terrainDesc = terrainsToSwitch[i];
                
                if (i == activeTerrainIndex)
                {
                    terrainDesc.terrain.SetActive(true);
                    terrainNameText.text = terrainDesc.name;
                    terrainDescText.text = terrainDesc.description;
                }
                else
                {
                    terrainDesc.terrain.SetActive(false);
                }
            }
        }
    }

    [Serializable]
    public class TerrainDesc
    {
        public GameObject terrain;
        public string name;
        public string description;
    }
}