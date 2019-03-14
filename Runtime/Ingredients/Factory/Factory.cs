using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameplayIngredients
{
    public class Factory : MonoBehaviour
    {
        public enum BlueprintSelectionMode
        {
            Random,
            Sequential,
            Shuffle
        }

        public enum SpawnTargetSelection
        {
            OneSequential,
            OneRandom,
            All
        }

        public enum SpawnLocation
        {
            Default,
            SameSceneAsTarget,
            ChildOfTarget,
            DontDestroyOnLoad
        }

        [ReorderableList, NonNullCheck]
        public GameObject[] FactoryBlueprints;
        [NonNullCheck]
        public GameObject SpawnTarget;

        public BlueprintSelectionMode blueprintSelecionMode = BlueprintSelectionMode.Random;

        public bool RespawnTarget = true;
        public SpawnLocation spawnLocation = SpawnLocation.SameSceneAsTarget;
        public float RespawnDelay = 3.0f;
        public bool ReapInstancesOnDestroy = true;

        [Min(1), SerializeField]
        private int MaxInstances = 1;

        [ReorderableList]
        public Callable[] OnSpawn;
        [ReorderableList]
        public Callable[] OnRespawn;

        List<GameObject> m_Instances;

        private void OnDestroy()
        {
            if(ReapInstancesOnDestroy)
            {
                foreach(var instance in m_Instances)
                {
                    if (instance != null)
                        Destroy(instance);
                }
            }
        }

        public void SetTarget(GameObject target)
        {
            if(target != null)
            {
                SpawnTarget = target;
            }
        }

        public void Spawn()
        {
            if(SpawnTarget == null || FactoryBlueprints == null  || FactoryBlueprints.Length == 0)
            {
                Debug.LogWarning(string.Format("Factory '{0}' : Cannot spawn as there are no spawn target or factory blueprints", gameObject.name));
                return;
            }

            if (m_Instances == null)
                m_Instances = new List<GameObject>();

            if (m_Instances.Count < MaxInstances)
            {
                GameObject newInstance = Spawn(SelectBlueprint(), SpawnTarget);

                switch(spawnLocation)
                {
                    case SpawnLocation.Default:
                        break;
                    case SpawnLocation.SameSceneAsTarget:
                        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene( newInstance, SpawnTarget.scene);
                        break;
                    case SpawnLocation.ChildOfTarget:
                        newInstance.transform.parent = SpawnTarget.transform;
                        break;
                    case SpawnLocation.DontDestroyOnLoad:
                        DontDestroyOnLoad(newInstance);
                        break;
                }

                m_Instances.Add(newInstance);
                
                Callable.Call(OnSpawn, newInstance);
            }

        }

        private void Update()
        {
            if(m_Instances != null)
            {
                List<int> todelete = new List<int>();
                for(int i = 0; i < m_Instances.Count; i++)
                {
                    if(m_Instances[i] == null)
                    {
                        todelete.Add(i);
                    }
                }

                foreach (var index in todelete)
                {
                    m_Instances.RemoveAt(index);
                    AddRespawnCoroutine();
                }
            }
        }

        private List<Coroutine> m_RespawnCoroutines; 

        private void AddRespawnCoroutine()
        {
            if (m_RespawnCoroutines == null)
                m_RespawnCoroutines = new List<Coroutine>();
            else
            {
                m_RespawnCoroutines.RemoveAll(o => o == null);
            }

            m_RespawnCoroutines.Add(StartCoroutine(Respawn(RespawnDelay)));
        }

        private IEnumerator Respawn(float time)
        {
            yield return new WaitForSeconds(time);
            Callable.Call(OnRespawn, this.gameObject);
            Spawn();
        }

        private GameObject Spawn(GameObject blueprint, GameObject target)
        {
            var Go = Instantiate(blueprint, target.transform.position, target.transform.rotation);
            Go.name = (blueprint.name);
            return Go;
        }

        int currentBlueprintIndex = -1;

        private GameObject SelectBlueprint()
        {
            switch(blueprintSelecionMode)
            {
                case BlueprintSelectionMode.Random:
                    currentBlueprintIndex = Random.Range(0, FactoryBlueprints.Length);
                    break;
                case BlueprintSelectionMode.Sequential:
                    currentBlueprintIndex = (currentBlueprintIndex++) % FactoryBlueprints.Length;
                    break;
                case BlueprintSelectionMode.Shuffle:
                    currentBlueprintIndex = Shuffle(currentBlueprintIndex);
                    break;
            }
            return FactoryBlueprints[currentBlueprintIndex];
        }

        List<int> shuffleIndices;

        private int Shuffle(int i)
        {
            if(shuffleIndices == null || shuffleIndices.Count != FactoryBlueprints.Length)
            {
                shuffleIndices = Enumerable.Range(0, FactoryBlueprints.Length).OrderBy(x => Random.value).ToList();
            }
            return shuffleIndices[(shuffleIndices.IndexOf(i) + 1) % shuffleIndices.Count];
        }
    }
}