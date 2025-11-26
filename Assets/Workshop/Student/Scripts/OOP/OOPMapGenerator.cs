using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{

    public class OOPMapGenerator : MonoBehaviour
    {
        [Header("Set MapGenerator")]
        public int X;
        public int Y;

        [Header("Set Player")]
        public OOPPlayer player;
        public Vector2Int playerStartPos;
        [Header("Set NPC")]
        public NPC Npc;
        public NPCSkill NpcSkill;
        [Header("Set Exit")]
        public OOPExit Exit;
        [Header("Set Wall")]
        public Identity Wall;

        [Header("Set Prefab")]
        public GameObject[] floorsPrefab;
        public GameObject[] wallsPrefab;
        public GameObject[] demonWallsPrefab;
        public GameObject[] itemsPrefab;
        public GameObject[] collectItemsPrefab;
        public GameObject[] EnemyPrefab;
        public GameObject[] SkillPrefab;

        [Header("Set Transform")]
        public Transform floorParent;
        public Transform wallParent;
        public Transform itemParent;
        public Transform enemyParent;

        [Header("Set object Count")]
        public int obsatcleCount;
        public int itemPotionCount;
        public int colloctItemCount;
        public int EnemyCount;
        public int SkillCount;

        public Identity[,] mapdata;
        [Header("Enemy on Map")]
        public List<OOPEnemy> EnemysOnMap = new List<OOPEnemy>();

        // block types ...
        [HideInInspector]
        public string empty = "";
        [HideInInspector]
        public string demonWall = "demonWall";
        [HideInInspector]
        public string potion = "potion";
        [HideInInspector]
        public string bonuesPotion = "bonuesPotion";
        [HideInInspector]
        public string exit = "exit";
        [HideInInspector]
        public string playerOnMap = "player";
        [HideInInspector]
        public string collectItem = "collectItem";
        [HideInInspector]
        public string enemy = "enemy";
        [HideInInspector]
        public string npc = "Npc";

        // Start is called before the first frame update
        private void Awake()
        {
            CreateMap();
        }
        void Start()
        {
            StartCoroutine(SetUPMap());
        }
        IEnumerator SetUPMap() {
            SetUpPlayer();
            SetUpExit();
            SetUpNpc();
            PlaceItemsOnMap(obsatcleCount, demonWallsPrefab, wallParent, demonWall);
            //PlaceItemsOnMap(itemPotionCount, itemsPrefab, itemParent, potion);
            PlaceItemsOnMap(colloctItemCount, collectItemsPrefab, itemParent, collectItem);
            PlaceItemsOnMap(SkillCount, SkillPrefab, itemParent, collectItem);
            //PlaceItemsOnMap(EnemyCount, EnemyPrefab, enemyParent, enemy);
            yield return new WaitForSeconds(0.5f);
            RandomDamageToListEnemies();
        }

        private void PlaceItemsOnMap(int count, GameObject[] prefab, Transform parent, string itemType, System.Action onComplete = null)
        {
            int placedCount = 0;
            int preventInfiniteLoop = 1000; // Increased loop prevention for safety

            while (placedCount < count)
            {
                if (--preventInfiniteLoop < 0)
                {
                    Debug.LogWarning("Could not place all items. Map may be too full.");
                    break;
                }

                int x = UnityEngine.Random.Range(0, X);
                int y = UnityEngine.Random.Range(0, Y);

                if (mapdata[x, y] == null)
                {
                    SetUpItem(x, y, prefab, parent, itemType);
                    placedCount++;
                }
            }
            onComplete?.Invoke();
        }
        private void SetUpNpc()
        {
            mapdata[Npc.positionX, Npc.positionY] = Npc;
            mapdata[NpcSkill.positionX, NpcSkill.positionY] = NpcSkill;
            Npc.transform.position = new Vector3(Npc.positionX, Npc.positionY, 0);
            NpcSkill.transform.position = new Vector3(NpcSkill.positionX, NpcSkill.positionY, 0);
            Npc.mapGenerator = this;
            NpcSkill.mapGenerator = this;
        }
        private void SetUpExit()
        {
            mapdata[X - 1, Y - 1] = Exit;
            Exit.transform.position = new Vector3(X - 1, Y - 1, 0);
        }

        private void SetUpPlayer()
        {
            player.mapGenerator = this;
            player.positionX = playerStartPos.x;
            player.positionY = playerStartPos.y;
            player.transform.position = new Vector3(playerStartPos.x, playerStartPos.y, -0.1f);
            mapdata[playerStartPos.x, playerStartPos.y] = player;
        }

        private void CreateMap()
        {
            mapdata = new Identity[X, Y];
            for (int x = -1; x < X + 1; x++)
            {
                for (int y = -1; y < Y + 1; y++)
                {
                    if (x == -1 || x == X || y == -1 || y == Y)
                    {
                        int r = Random.Range(0, wallsPrefab.Length);
                        GameObject obj = Instantiate(wallsPrefab[r], new Vector3(x, y, 0), Quaternion.identity);
                        obj.transform.parent = wallParent;
                        obj.name = "Wall_" + x + ", " + y;
                    }
                    else
                    {
                        int r = Random.Range(0, floorsPrefab.Length);
                        GameObject obj = Instantiate(floorsPrefab[r], new Vector3(x, y, 1), Quaternion.identity);
                        obj.transform.parent = floorParent;
                        obj.name = "floor_" + x + ", " + y;
                        mapdata[x, y] = null;
                    }
                }
            }
        }

        public Identity GetMapData(float x, float y)
        {
            if (x >= X || x < 0 || y >= Y || y < 0) {
                return Wall;
            }

            return mapdata[(int)x, (int)y];
        }

        public void SetUpItem(int x, int y,GameObject[] _itemsPrefab,Transform parrent,string _name)
        {
            int r = Random.Range(0, _itemsPrefab.Length);
            GameObject obj = Instantiate(_itemsPrefab[r], new Vector3(x, y, 0), Quaternion.identity);
            obj.transform.parent = parrent;
            mapdata[x, y] = obj.GetComponent<Identity>();
            mapdata[x, y].positionX = x;
            mapdata[x, y].positionY = y;
            mapdata[x, y].mapGenerator = this;
            if (_name != collectItem) {
                mapdata[x, y].Name = _name;
            }
            if (_name == enemy) {
                EnemysOnMap.Add(obj.GetComponent<OOPEnemy>());
            }
            obj.name = $"Object_{mapdata[x, y].Name} {x}, {y}";
        }
        public void SetUpItem(int x, int y, GameObject _itemsPrefab, Transform parrent, string _name)
        {
            _itemsPrefab.transform.parent = parrent;
            mapdata[x, y] = _itemsPrefab.GetComponent<Identity>();
            mapdata[x, y].positionX = x;
            mapdata[x, y].positionY = y;
            mapdata[x, y].mapGenerator = this;
            if (_name != collectItem)
            {
                mapdata[x, y].Name = _name;
            }
            if (_name == enemy)
            {
                EnemysOnMap.Add(_itemsPrefab.GetComponent<OOPEnemy>());
            }
            _itemsPrefab.name = $"Object_{mapdata[x, y].Name} {x}, {y}";
        }

        public OOPEnemy[] GetEnemies()
        {
            return EnemysOnMap.ToArray();
        }
        public void MoveEnemies()
        {
            foreach (var enemy in EnemysOnMap)
            {
                enemy.RandomMove();
            }
        }
        public void RandomDamageToListEnemies() {
            Debug.Log($"Damage to {EnemysOnMap.Count} EnemysOnMap");
            foreach (var enemy in EnemysOnMap)
            {
                int damage = Random.Range(1, 15);
                enemy.TakeDamage(damage);
            }
        }
    }
}