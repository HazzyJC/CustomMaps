using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using HBMF;
using BulletMenuVR;
using System.Reflection;
using UnityEngine.AI;

namespace CustomMaps
{
    public class CustomMapsClass : MelonMod
    {
        public bool maploading = false;
        public int whichMapLoading = 0;
        public int enemAmm = 4;
        public int enemDead = 3;
        public bool isCustomMapsActive = false;
        public override void OnApplicationStart()
        {
            //-- Shows Directory
            Directory.CreateDirectory(MelonUtils.UserDataDirectory + "\\CustomMaps");
            Directory.CreateDirectory(MelonUtils.UserDataDirectory + "\\ModManager");
            string[] dirs3 = Directory.GetFiles(String.Format("{0}\\CustomMaps", MelonUtils.UserDataDirectory), "*.hbm");

            string[] dirs2 = Directory.GetDirectories(MelonUtils.UserDataDirectory + "\\ModManager");

            List<string> ls = dirs3.ToList();

            foreach (string BaseDirectory in dirs2.ToList())
            {
                string[] str = Directory.GetFiles(String.Format("{0}\\CustomMaps", BaseDirectory), "*.hbm");
                if (str.Length == 0) { MelonLogger.Msg("No map files found"); continue; }

                foreach (string map in str)
                {
                    ls.Add(map);
                }
            }

            VrMenuPageBuilder pageBuilder = VrMenuPageBuilder.Builder();
            int i = 0;

            string[] dirs = ls.ToArray();

            foreach (string mapName in dirs)
            {
                int bruh = i;
                pageBuilder.AddButton(new VrMenuButton(Path.GetFileName(dirs[i]), () =>
                {
                    maploading = true;
                    whichMapLoading = bruh;
                    SceneManager.LoadScene(sceneBuildIndex: 2);
                }));
                i++;
            }

            VrMenuPageBuilder wavePageBuilder = VrMenuPageBuilder.Builder();

            wavePageBuilder.AddButton(new VrMenuButton("Spawn Waves", () =>
            {
                InfinityWaveSpawner infWave = GameObject.Find("InfinityWaveSpawner").GetComponent<InfinityWaveSpawner>();
                infWave.StartSpawnEnemies();
            }, Color.red));

            wavePageBuilder.AddButton(new VrMenuButton("Stop Waves", () =>
            {
                InfinityWaveSpawner infWave = GameObject.Find("InfinityWaveSpawner").GetComponent<InfinityWaveSpawner>();
                infWave.StopSpawnEnemies();
            }, Color.red));

            wavePageBuilder.AddButton(new VrMenuButton("Enemies: 1", () =>
            {
                InfinityWaveSpawner infWave = GameObject.Find("InfinityWaveSpawner").GetComponent<InfinityWaveSpawner>();
                infWave._enemiesCount = 1;
            }, Color.black));

            wavePageBuilder.AddButton(new VrMenuButton("Enemies: 2", () =>
            {
                InfinityWaveSpawner infWave = GameObject.Find("InfinityWaveSpawner").GetComponent<InfinityWaveSpawner>();
                infWave._enemiesCount = 2;
            }, Color.black));

            wavePageBuilder.AddButton(new VrMenuButton("Enemies: 3", () =>
            {
                InfinityWaveSpawner infWave = GameObject.Find("InfinityWaveSpawner").GetComponent<InfinityWaveSpawner>();
                infWave._enemiesCount = 3;
            }, Color.black));

            wavePageBuilder.AddButton(new VrMenuButton("Enemies: 4", () =>
            {
                InfinityWaveSpawner infWave = GameObject.Find("InfinityWaveSpawner").GetComponent<InfinityWaveSpawner>();
                infWave._enemiesCount = 4;
            }, Color.black));

            VrMenuPage wavePage = wavePageBuilder.Build();

            pageBuilder.AddButton(new VrMenuButton("Waves", () =>
            {
                wavePage.Open();
            }, Color.red));

            VrMenuPage menuPage = pageBuilder.Build();
            VrMenu.RegisterMainButton(new VrMenuButton("Custom Maps", () =>
            {
                menuPage.Open();
            }, Color.blue));

            MelonLogger.Msg("You have {0} Custom Map(s) installed.", dirs.Length);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (maploading)
            {
                maploading = false;
                CreateMap.SpawnMap(whichMapLoading);
            }
        }

        public static void Catcher()
        {
            CustomMapsClass cmp = new CustomMapsClass();
            MelonCoroutines.Start(DestroyPlayground());
        }

        public static IEnumerator DestroyPlayground()
        {
            yield return new WaitForSeconds(1);
            GameObject Map = GameObject.Find("[SCENE]/Environment/StaticGeometry");
            GameObject Map2 = GameObject.Find("[SCENE]/Environment/Light");
            GameObject Map3 = GameObject.Find("[SCENE]/Light Probe Group");
            Map.SetActive(false);
            Map2.SetActive(false);
            Map3.SetActive(false);
        }
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                CreateMap.SpawnMap(0);
            }
        }
    }

    public class CreateMap : MonoBehaviour
    {
        public static AssetBundle LastLoad;
        public static GameObject playerOne;

        public static GameObject lastMap;
        public static NavMeshDataInstance lastNavMesh;
        public static bool hasNavMesh = false;
        public static bool hasSkyBox = false;
        public static Material sky;

        public static Transform spawnTrans;

        public static bool mapSpawned = false;

        public static int prevLoaded = 100;

        public static AssetBundle localAssetBundle;
        public static void SpawnMap(int mapNum)
        {
            playerOne = GameObject.Find("[HARD BULLET PLAYER]");
            if (lastMap != null)
            {
                Destroy(lastMap);
            }
            if (hasNavMesh)
            {
                NavMesh.RemoveNavMeshData(lastNavMesh);
            }


            string[] dirs3 = Directory.GetFiles(String.Format("{0}\\CustomMaps", MelonUtils.UserDataDirectory), "*.hbm");

            string[] dirs2 = Directory.GetDirectories(MelonUtils.UserDataDirectory + "\\ModManager");

            List<string> ls = dirs3.ToList();

            foreach (string BaseDirectory in dirs2.ToList())
            {
                string[] str = Directory.GetFiles(String.Format("{0}\\CustomMaps", BaseDirectory), "*.hbm");
                if (str.Length == 0) { MelonLogger.Msg("No map files found"); continue; }

                foreach (string map in str)
                {
                    ls.Add(map);
                }
            }
            
            string[] dirs = ls.ToArray();
            string mapName = dirs[mapNum];
            Destroy(GameObject.Find("[HARD BULLET PLAYER]/PlayerSystems/OutOfBoundsSceneRestart"));

            //Spawns The Map
            localAssetBundle = AssetBundle.LoadFromFile(mapName);
            LastLoad = localAssetBundle;


            if (localAssetBundle == null)
            {
                MelonLogger.Msg("Failed :((((((");
                return;
            }

            GameObject asset = localAssetBundle.LoadAsset<GameObject>("CustomMap");
            NavMeshData navMesh = localAssetBundle.LoadAsset<NavMeshData>("NavMesh");
            if (navMesh != null)
            {
                hasNavMesh = true;
                NavMeshData navmash = Instantiate(navMesh);
                lastNavMesh = NavMesh.AddNavMeshData(navmash);
            }
            lastMap = Instantiate(asset);
            localAssetBundle.Unload(false);

            //tp scene
            GameObject scene = GameObject.Find("[SCENE]");
            scene.transform.position = new Vector3(0, 500, 0);
            GameObject.Destroy(GameObject.Find("[SCENE]/Environment/StaticGeometry/"));

            //View Distance;
            Camera cam = GameObject.Find("[HARD BULLET PLAYER]/HexaBody/Pelvis/CameraRig/FloorOffset/Scaler/Camera").GetComponent<Camera>();
            Camera recCam = GameObject.Find("[HARD BULLET PLAYER]/HexaBody/Pelvis/CameraRig/FloorOffset/Scaler/Camera/RecordingCamera").GetComponent<Camera>();

            recCam.farClipPlane = 10000f;
            cam.farClipPlane = 10000f;

            if (cam == null)
            {
                MelonLogger.Warning("cam is null lmfao cryingemoji x 7");
            }
            else
            {
                GameObject spawnPoint = GameObject.Find("CustomMap(Clone)/spawnpoint");
                if (spawnPoint == null)
                {
                    MelonLogger.Warning("Cannot Find Spawn Point!");
                }
                else
                {
                    playerOne.transform.position = spawnPoint.transform.position;
                }
            }

            GameObject enemyPrefab = null;

            foreach (EnemySpawnerFromGenerator checkedSpawner in Resources.FindObjectsOfTypeAll<EnemySpawnerFromGenerator>())
            {
                if (!checkedSpawner.gameObject.activeInHierarchy)
                {
                    continue;
                }

                enemyPrefab = ReflectionHelper.GetPrivateField<GameObject>(checkedSpawner, "_spawnPrefab");
                if (enemyPrefab != null)
                {
                    break;
                }
            }

            if (enemyPrefab == null)
            {
                MelonLogger.Msg("fartballs");
            }

            //spawnpoints
            GameObject.Find("SpawnPoint").transform.position = GameObject.Find("sp1").transform.position;
            GameObject.Find("SpawnPoint (1)").transform.position = GameObject.Find("sp2").transform.position;
            GameObject.Find("SpawnPoint (2)").transform.position = GameObject.Find("sp3").transform.position;
            GameObject.Find("SpawnPoint (3)").transform.position = GameObject.Find("sp4").transform.position;
            GameObject.Find("SpawnPoint (4)").transform.position = GameObject.Find("sp5").transform.position;
            GameObject.Find("SpawnPoint (5)").transform.position = GameObject.Find("sp6").transform.position;


            GameObject[] balls = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject cum in balls)
            {
                if (cum.name.Contains("Enemspawn"))
                {
                    GameObject enemyReal = Instantiate(enemyPrefab, cum.transform.position, Quaternion.identity);
                    enemyReal.GetComponent<EnemyRoot>().Blackboard.SetVariableValue("Target", GameObject.Find("[HARD BULLET PLAYER]").GetComponent<PlayerRoot>().PlayerHead);
                }
            }
        }
    }
    public class ReflectionHelper
    {
        public static T GetPrivateField<T>(object fieldHolder, string fieldName)
        {
            if (fieldHolder == null)
            {
                return default;
            }

            Type type = fieldHolder.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo == null)
            {
                MelonLogger.Error("Attempted to get a private field which does not exist.");
                return default;
            }

            return (T)fieldInfo.GetValue(fieldHolder);
        }
    }

}