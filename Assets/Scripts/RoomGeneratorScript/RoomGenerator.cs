using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{

    #region SeparateScripts
    public InteractivePlacer interactivePlacer;
    [Header("Enemy Buckets")]
    public EnemyBuckets enemyBucket;

    public PlayerRecordsTracker playerRecords;
    #endregion


    public float overlayStrictPercentReduction = 0.1f;
    public float specialStructurePercentile = 0.3f;
    int roomsPlaced = 1;

    [Header("Room Prefabs")]
    public List<GameObject> roomPrefabs;
    public int numberOfRooms = 10;
    public GameObject squarePrefab;
    public float distanceOffset = 5.0f;
    public List<GameObject> bossRoomPrefabs;
    private bool bossRoomPlaced = false;
    private Vector2Int bossRoomPos = Vector2Int.zero;

    [Header("Special Rooms")]
    public GameObject safeRoomPrefab;
    private bool safeRoomPlaced = false;
    [Header("Key Prefabs")]
    public GameObject silverKeyPrefab;
    public GameObject goldKeyPrefab;
    public GameObject purpleKeyPrefab;

    [Header("Lock Settings")]
    public int minLocks = 2;
    public int maxLocks = 4;

    [Header("Item & Enemy Prefabs")]
    public List<GameObject> commonItemPrefabs;
    public List<GameObject> significantItemPrefabs;
    [Range(0, 10)] public int maxRareItemCount = 16;
    public GameObject bossEnemyPrefab;
    private HashSet<Vector2Int> rareItemRooms = new HashSet<Vector2Int>();

    public int minItemPerRoom = 0;
    public int maxItemPerRoom = 2;
    private Dictionary<float, GameObject> roomData;


    [Header("Enemy Spawn Settings")]
    [Range(0f, 1f)] public float enemySpawnChance = 0.3f;
    [Range(0f, 1f)] public float toughEnemyChance = 0.2f;
    public int minEnemiesPerRoom = 0;
    public int maxEnemiesPerRoom = 3;
    public int maxBossCount = 1;

    [Header("Portal Prefab")]
    public GameObject portalPrefab;

    private Dictionary<Vector2Int, GameObject> placedRooms = new Dictionary<Vector2Int, GameObject>();
    private List<Vector2Int> frontier = new List<Vector2Int>();
    private List<Vector2Int> roomPlacementOrder = new List<Vector2Int>();
    private Dictionary<Vector2Int, int> roomLockCount = new Dictionary<Vector2Int, int>();
    private List<KeyLockInfo> lockedExits = new List<KeyLockInfo>();
    private Vector2Int portalPos = Vector2Int.zero;

    public List<Vector2Int> deadEndRooms = new List<Vector2Int>();

    //BFS for enemy placement
    Dictionary<Vector2Int, int> roomDistanceFromStart = new Dictionary<Vector2Int, int>();

    class KeyLockInfo
    {
        public Vector2Int pos;
        public string direction;
        public string lockType;
    }

    private Vector2Int[] directions = new Vector2Int[]
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    private string[] directionNames = new string[]
    {
        "Top", "Right", "Bottom", "Left"
    };

    //private void Start()
    //{
    //    GenerateLevel();
    //}

    private void Start()
    {
        playerRecords = PlayerRecordsTracker.instance;
        //StartCoroutine(GenerateLevelCoroutine());
        //GenerateLevel();
        TryGenerateLevel();
    }

    //For debugging use
    IEnumerator GenerateLevelCoroutine()
    {
        placedRooms.Clear();
        frontier.Clear();
        roomPlacementOrder.Clear();
        roomLockCount.Clear();
        lockedExits.Clear();

        Vector2Int startPos = Vector2Int.zero;
        GameObject startRoom = PlaceRoom(startPos);
        frontier.Add(startPos);
        roomPlacementOrder.Add(startPos);
        int locksToPlace = Random.Range(minLocks, maxLocks + 1);
        int locksPlaced = 0;

        if (startRoom.tag != "LoopStructure")
        {
            startRoom.name = $"Room_{roomsPlaced}";
        }

        yield return new WaitForSeconds(0.2f);

        List<int> specialStructuresToBuild = new List<int> { 0, 1, 2, 3 }; 

        Vector2Int lastRoomPos = Vector2Int.zero;
        string lastIncomingDir = null;

        while (roomsPlaced < numberOfRooms && frontier.Count > 0)
        {
            Vector2Int currentPos = frontier[Random.Range(0, frontier.Count)];
            Vector2Int newDir = GetRandomAvailableDirection(currentPos);
            if (newDir == Vector2Int.zero)
            {
                frontier.Remove(currentPos);
                continue;
            }

            Vector2Int newPos = currentPos + newDir;
            if (placedRooms.ContainsKey(newPos)) continue;

            string incomingDir = GetDirectionName(-newDir);
            string fromDir = GetDirectionName(newDir);

            GameObject currentRoom = placedRooms[currentPos];
            RoomData currentData = currentRoom.GetComponent<RoomData>();
            GameObject newRoom = PlaceRoom(newPos, incomingDir, currentData, fromDir);

            if (newRoom == null)
            {
                continue;
            }

            roomsPlaced++;
            roomPlacementOrder.Add(newPos);

            if (newRoom.tag != "LoopStructure")
            {
                newRoom.name = $"Room_{roomsPlaced}";
            }

            string doorType = "";

            bool currentLocked = roomLockCount.ContainsKey(currentPos);
            bool newLocked = roomLockCount.ContainsKey(newPos);

            Vector2Int keyCandidatePos = currentPos;

            if (locksPlaced < locksToPlace &&
                currentPos != Vector2Int.zero && newPos != Vector2Int.zero &&
                !currentLocked && !newLocked &&
                CanPlaceKeyAt(keyCandidatePos))
            {
                string candidateLock = GetRandomExitType();
                if (Random.Range(0, 2) == 0 || !string.IsNullOrEmpty(candidateLock))
                {
                    doorType = string.IsNullOrEmpty(candidateLock) ? GetRandomExitType(true) : candidateLock;
                    KeyLockInfo info = new KeyLockInfo();
                    info.pos = newPos;
                    info.direction = incomingDir;
                    info.lockType = doorType;
                    lockedExits.Add(info);
                    roomLockCount[newPos] = 1;
                    roomLockCount[currentPos] = 1;
                    locksPlaced++;
                }
            }

            DisableWall(currentPos, fromDir);
            DisableWall(newPos, incomingDir);
            EnableDoor(currentPos, fromDir, doorType);
            EnableDoor(newPos, incomingDir, "");

            frontier.Add(newPos);

            lastRoomPos = currentPos;
            lastIncomingDir = fromDir;

            if (specialStructuresToBuild.Count > 0)
            {
                int roomsRemaining = numberOfRooms - roomsPlaced;

                for (int i = 0; i < specialStructuresToBuild.Count; i++)
                {
                    int type = specialStructuresToBuild[i];

                    // Only allow ForkStructure to be placed when nearing end (e.g., 5 or fewer rooms left)
                    if (type == 2 && roomsRemaining > 5)
                        continue;

                    specialStructuresToBuild.RemoveAt(i);
                    Debug.Log("Generating Guaranteed Special Structure");

                    yield return StartCoroutine(GenerateSpecialStructureByType(type, currentPos));
                    break;
                }
            }


            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.3f);



        PlaceKeys();
        ComputeRoomDistancesFromStart();
        PlaceItemsAndEnemies();
        interactivePlacer.PlaceInteractives(placedRooms, roomPlacementOrder);

        // Enable purple door on the last placed connection
        if (lastRoomPos != Vector2Int.zero && !string.IsNullOrEmpty(lastIncomingDir))
        {
            EnablePurpleDoor(lastRoomPos, lastIncomingDir);
            //EnablePurpleDoor(roomPlacementOrder[roomPlacementOrder.Count - 1], GetOppositeDirection(lastIncomingDir));
            Debug.Log($"Purple door set between {lastRoomPos} and {roomPlacementOrder[roomPlacementOrder.Count - 1]}");
        }

        // Place portal in last normal room
        for (int i = roomPlacementOrder.Count - 2; i >= 0; i--)
        {
            Vector2Int pos = roomPlacementOrder[i];
            GameObject room = placedRooms[pos];

            if (room.tag != "LoopStructure" && room.tag != "BossRoom")
            {
                Instantiate(portalPrefab, room.transform.position, Quaternion.identity);
                portalPos = pos; // correct grid key
                room.tag = "PortalRoom";
                Debug.Log($"Portal placed at {room.name}");
                break;
            }
        }

        CleanEnemiesNearPortal();
    }

    void GenerateLevel()
    {
        placedRooms.Clear();
        frontier.Clear();
        roomPlacementOrder.Clear();
        roomLockCount.Clear();
        lockedExits.Clear();
        roomsPlaced = 0;
        deadEndRooms.Clear();

        bossRoomPlaced = false;
        safeRoomPlaced = false;
        bossRoomPos = Vector2Int.zero;

        Vector2Int startPos = Vector2Int.zero;
        GameObject startRoom = TryPlaceRoom(startPos, GetValidStartRoomPrefab(), null, null, null);

        if (startRoom == null)
        {
            Debug.LogError("[LevelGen] Failed to place a valid start room.");
            return;
        }

        frontier.Add(startPos);
        roomPlacementOrder.Add(startPos);
        int locksToPlace = Random.Range(minLocks, maxLocks + 1);
        int locksPlaced = 0;

        if (startRoom.tag != "LoopStructure")
        {
            startRoom.name = $"Room_{roomsPlaced}";
        }

        List<int> specialStructuresToBuild = new List<int> { 0, 1, 2, 3 };

        Vector2Int lastRoomPos = Vector2Int.zero;
        string lastIncomingDir = null;

        while (roomsPlaced < numberOfRooms && frontier.Count > 0)
        {
            Vector2Int currentPos = frontier[Random.Range(0, frontier.Count)];
            Vector2Int newDir = GetRandomAvailableDirection(currentPos);
            if (newDir == Vector2Int.zero)
            {
                frontier.Remove(currentPos);
                continue;
            }

            Vector2Int newPos = currentPos + newDir;
            if (placedRooms.ContainsKey(newPos)) continue;

            string incomingDir = GetDirectionName(-newDir);
            string fromDir = GetDirectionName(newDir);

            GameObject currentRoom = placedRooms[currentPos];
            RoomData currentData = currentRoom.GetComponent<RoomData>();

            // ==== Boss room logic ====
            GameObject prefabToUse = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            GameObject newRoom = TryPlaceRoom(newPos, prefabToUse, incomingDir, currentData, fromDir);

            if (newRoom == null)
            {
                newRoom = PlaceSquareRoom(newPos, squarePrefab, incomingDir, currentData, fromDir);
                if (newRoom == null) continue;
            }

            // Success
            roomsPlaced++;
            roomPlacementOrder.Add(newPos);

            if (newRoom.tag != "LoopStructure")
            {
                newRoom.name = $"Room_{roomsPlaced}";
            }

            // === Lock + Door Setup ===
            string doorType = "";

            bool currentLocked = roomLockCount.ContainsKey(currentPos);
            bool newLocked = roomLockCount.ContainsKey(newPos);
            Vector2Int keyCandidatePos = currentPos;

            if (locksPlaced < locksToPlace &&
                currentPos != Vector2Int.zero && newPos != Vector2Int.zero &&
                !currentLocked && !newLocked &&
                CanPlaceKeyAt(keyCandidatePos))
            {
                string candidateLock = GetRandomExitType();
                if (Random.Range(0, 2) == 0 || !string.IsNullOrEmpty(candidateLock))
                {
                    doorType = string.IsNullOrEmpty(candidateLock) ? GetRandomExitType(true) : candidateLock;
                    KeyLockInfo info = new KeyLockInfo();
                    info.pos = newPos;
                    info.direction = incomingDir;
                    info.lockType = doorType;
                    lockedExits.Add(info);
                    roomLockCount[newPos] = 1;
                    roomLockCount[currentPos] = 1;
                    locksPlaced++;
                }
            }

            DisableWall(currentPos, fromDir);
            DisableWall(newPos, incomingDir);
            EnableDoor(currentPos, fromDir, doorType);
            EnableDoor(newPos, incomingDir, "");

            frontier.Add(newPos);
            lastRoomPos = currentPos;
            lastIncomingDir = fromDir;

            if (specialStructuresToBuild.Count > 0)
            {
                int roomsRemaining = numberOfRooms - roomsPlaced;

                for (int i = 0; i < specialStructuresToBuild.Count; i++)
                {
                    int type = specialStructuresToBuild[i];

                    // Add guards for specific structures
                    if (type == 2 && roomsRemaining > 5)
                        continue;
                    if (type == 3 && roomsPlaced < numberOfRooms * 0.5f) // SafeRoom
                        continue;

                    specialStructuresToBuild.RemoveAt(i);
                    Debug.Log("Generating Guaranteed Special Structure");

                    switch (type)
                    {
                        case 0: GenerateLoopSync(); break;
                        case 1: GenerateBranchSync(); break;
                        case 2: GenerateForkStructureSync(currentPos); break;
                        case 3: GenerateSafeRoomSync(currentPos); break;
                    }
                    break;
                }

            }
        }

        // === Post-placement logic ===
        if (!bossRoomPlaced && roomsPlaced >= (int)(numberOfRooms * 0.7f))
        {
            bool success = ForcePlaceBossRoom();
            if (!success)
            {
                Debug.LogError("[LevelGen] Could not force place boss room. Regenerating...");
                placedRooms.Clear();
                roomPlacementOrder.Clear();
                return;
            }
            else if (!bossRoomPlaced)
            {
                Debug.LogError("[LevelGen] Not enough rooms to place boss room. Retrying...");
                placedRooms.Clear();
                roomPlacementOrder.Clear();
                return;
            }
        }

        bool safeRoomSuccess = false;
        int retryAttempts = 10;
        for (int i = 0; i < retryAttempts && !safeRoomSuccess; i++)
        {
            Vector2Int origin = roomPlacementOrder[Random.Range(numberOfRooms / 2, numberOfRooms - 3)];
            safeRoomSuccess = GenerateSafeRoomSync(origin);
        }

        if (!safeRoomSuccess)
            Debug.LogWarning("Failed to place SafeRoom after retries!");

        PlaceKeys();
        ComputeRoomDistancesFromStart();
        PlaceItemsAndEnemies();
        interactivePlacer.PlaceInteractives(placedRooms, roomPlacementOrder);

        if (lastRoomPos != Vector2Int.zero && !string.IsNullOrEmpty(lastIncomingDir))
        {
            EnablePurpleDoor(lastRoomPos, lastIncomingDir);
            Debug.Log($"Purple door set between {lastRoomPos} and {roomPlacementOrder[roomPlacementOrder.Count - 1]}");
        }

        for (int i = roomPlacementOrder.Count - 2; i >= 0; i--)
        {
            Vector2Int pos = roomPlacementOrder[i];
            GameObject room = placedRooms[pos];

            if (room.tag != "LoopStructure" && room.tag != "BossRoom")
            {
                Instantiate(portalPrefab, room.transform.position, Quaternion.identity);
                portalPos = pos; // correct grid key
                Debug.Log($"Portal placed at {room.name}");
                break;
            }
        }

        CleanEnemiesNearPortal();
    }

    public void TryGenerateLevel(int maxAttempts = 100)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            Debug.Log($"[LevelGen] Attempt {attempt}...");
            GenerateLevel(); // synchronous generation

            bool generationValid = !LevelHasOverlap() && bossRoomPlaced;

            if (generationValid)
            {
                Debug.Log($"[LevelGen] Success on attempt {attempt}!");
                return;
            }


            Debug.LogWarning($"[LevelGen] Failed due to {(bossRoomPlaced ? "overlap" : "boss room issue")} on attempt {attempt}...");


            Debug.LogWarning($"[LevelGen] Overlap detected on attempt {attempt}, retrying...");

            // Destroy all spawned rooms
            foreach (GameObject room in placedRooms.Values)
            {
                if (room != null)
                    DestroyImmediate(room);
            }

            // Clear additional tags if necessary (enemy, item, key, portal)
            string[] tagsToClear = { "Enemy", "Item", "Key", "Portal", "Traps" };
            foreach (string tag in tagsToClear)
            {
                GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
                foreach (GameObject obj in objs)
                {
                    DestroyImmediate(obj);
                }
            }

            // Clear internal tracking
            placedRooms.Clear();
            roomPlacementOrder.Clear();
        }

        Debug.LogError("[LevelGen] Failed to generate a valid level without overlaps after max attempts.");
    }

    bool LevelHasOverlap()
    {
        List<Collider2D> allColliders = new List<Collider2D>();

        foreach (var room in placedRooms.Values)
        {
            var overlay = room.transform.Find("RoomOverlayCollider");
            if (overlay == null) continue;

            var colliders = overlay.GetComponentsInChildren<BoxCollider2D>();
            allColliders.AddRange(colliders);
        }

        for (int i = 0; i < allColliders.Count; i++)
        {
            Bounds a = allColliders[i].bounds;
            a.Expand(new Vector3(
                -overlayStrictPercentReduction * a.size.x,
                -overlayStrictPercentReduction * a.size.y,
                0f
            ));

            for (int j = i + 1; j < allColliders.Count; j++)
            {
                Bounds b = allColliders[j].bounds;
                b.Expand(new Vector3(
                    -overlayStrictPercentReduction * b.size.x,
                    -overlayStrictPercentReduction * b.size.y,
                    0f
                ));

                if (a.Intersects(b))
                {
                    Debug.LogWarning($"[Overlap] Room {i} and Room {j} are overlapping.");
                    return true;
                }
            }
        }

        return false;
    }

    void CleanEnemiesNearPortal()
    {
        if (!placedRooms.ContainsKey(portalPos)) return;

        Debug.Log("HI");
        Debug.Log(portalPos);

        GameObject portalRoom = placedRooms[portalPos];
        Collider2D[] overlaps = Physics2D.OverlapBoxAll(
            portalRoom.transform.position,
            new Vector2(10f, 10f), // Adjust size based on your room dimensions
            0f
        );

        foreach (Collider2D col in overlaps)
        {
            if (col.CompareTag("Enemy"))
            {
                Destroy(col.gameObject);
                Debug.Log($"[Cleanup] Removed enemy near portal in {portalRoom.name}");
            }
        }
    }

    IEnumerator GenerateSpecialStructureByType(int type, Vector2Int origin)
    {
        switch (type)
        {
            case 0:
                Debug.Log("Loop Structure");
                yield return StartCoroutine(GenerateLoop());
                break;
            case 1:
                Debug.Log("Branch Structure");
                yield return StartCoroutine(GenerateBranch());
                break;
            case 2:
                Debug.Log("Loop Structure");
                yield return StartCoroutine(GenerateForkStructure(origin));
                break;
            case 3:
                Debug.Log("Safe Room Structure");
                yield return StartCoroutine(GenerateSafeRoom(origin));
                break;

        }
    }

    bool CanPlaceKeyAt(Vector2Int pos)
    {
        return pos != Vector2Int.zero && placedRooms.ContainsKey(pos) && !roomLockCount.ContainsKey(pos);
    }
    GameObject PlaceSquareRoom(Vector2Int gridPos, GameObject prefab, string incomingDir = null, RoomData fromRoom = null, string fromDir = null)
    {
        Vector3 targetPosition;

        if (incomingDir != null && fromRoom != null && fromDir != null)
        {
            RoomData tempData = prefab.GetComponent<RoomData>();
            Transform thisAnchor = tempData.GetAnchor(incomingDir);
            Transform fromAnchor = fromRoom.GetAnchor(fromDir);

            if (thisAnchor && fromAnchor)
            {
                targetPosition = fromAnchor.position - thisAnchor.localPosition;
            }
            else
            {
                targetPosition = new Vector3(gridPos.x * distanceOffset, gridPos.y * distanceOffset, 0);
            }
        }
        else
        {
            targetPosition = new Vector3(gridPos.x * distanceOffset, gridPos.y * distanceOffset, 0);
        }

        if (WouldOverlapAtPosition(prefab, targetPosition))
        {
            return null;
        }

        GameObject room = Instantiate(prefab, targetPosition, Quaternion.identity);
        placedRooms[gridPos] = room;

        RoomData data = room.GetComponent<RoomData>();
        if (data == null)
        {
            Debug.LogError("Missing RoomData on prefab");
            return room;
        }

        for (int i = 0; i < 4; i++)
        {
            string dir = directionNames[i];
            Transform wall = room.transform.Find(dir + "Exit");
            if (wall) wall.gameObject.SetActive(true);
            Transform silverDoor = room.transform.Find(dir + "SilverDoor");
            if (silverDoor) silverDoor.gameObject.SetActive(false);
            Transform goldDoor = room.transform.Find(dir + "GoldDoor");
            if (goldDoor) goldDoor.gameObject.SetActive(false);
            Transform purpleDoor = room.transform.Find(dir + "PurpleDoor");
            if (purpleDoor) purpleDoor.gameObject.SetActive(false);
        }

        return room;
    }

    string GetOppositeDirection(string dir)
    {
        switch (dir)
        {
            case "Top": return "Bottom";
            case "Bottom": return "Top";
            case "Left": return "Right";
            case "Right": return "Left";
            default: return null;
        }
    }

    GameObject PlaceRoom(Vector2Int gridPos, string incomingDir = null, RoomData fromRoom = null, string fromDir = null)
    {
        GameObject roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];

        // Try main prefab
        GameObject room = TryPlaceRoom(gridPos, roomPrefab, incomingDir, fromRoom, fromDir);

        // If it fails due to overlap, try squarePrefab instead
        if (room == null)
        {
            room = PlaceSquareRoom(gridPos, squarePrefab, incomingDir, fromRoom, fromDir);
        }

        return room;
    }

    Vector2Int GetRandomAvailableDirection(Vector2Int pos)
    {
        List<Vector2Int> available = new List<Vector2Int>();
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int neighbor = pos + directions[i];
            if (!placedRooms.ContainsKey(neighbor))
                available.Add(directions[i]);
        }

        if (available.Count > 0)
            return available[Random.Range(0, available.Count)];
        return Vector2Int.zero;
    }

    Vector2Int? GetNextClockwiseDirection(Vector2Int pos)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int dir = directions[i];
            Vector2Int nextPos = pos + dir;
            if (!placedRooms.ContainsKey(nextPos))
                return dir;
        }
        return null;
    }

    string GetDirectionName(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return "Top";
        if (dir == Vector2Int.right) return "Right";
        if (dir == Vector2Int.down) return "Bottom";
        if (dir == Vector2Int.left) return "Left";
        return null;
    }

    void DisableWall(Vector2Int pos, string direction)
    {
        if (!placedRooms.ContainsKey(pos)) return;

        Transform wall = placedRooms[pos].transform.Find(direction + "Exit");
        if (wall) wall.gameObject.SetActive(false);
    }

    void EnableDoor(Vector2Int pos, string direction, string doorType)
    {
        if (!placedRooms.ContainsKey(pos)) return;

        string[] types = { "Silver", "Gold" };
        for (int i = 0; i < types.Length; i++)
        {
            string type = types[i];
            Transform door = placedRooms[pos].transform.Find(direction + type + "Door");
            if (door) door.gameObject.SetActive(type == doorType);
        }
    }

    void DisableWall(GameObject room, string direction)
    {
        Transform wall = room.transform.Find(direction + "Exit");
        if (wall) wall.gameObject.SetActive(false);
    }

    void EnableDoor(GameObject room, string direction, string doorType)
    {
        string[] types = { "Silver", "Gold" };
        for (int i = 0; i < types.Length; i++)
        {
            string type = types[i];
            Transform door = room.transform.Find(direction + type + "Door");
            if (door) door.gameObject.SetActive(type == doorType);
        }
    }

    // Update GetRandomExitType so it still randomly picks Silver/Gold (but never Purple here)
    string GetRandomExitType(bool force = false)
    {
        if (force)
        {
            return Random.Range(0, 2) == 0 ? "Silver" : "Gold";
        }

        int roll = Random.Range(0, 3); // 0: no lock, 1: silver, 2: gold
        return roll == 0 ? "" : (roll == 1 ? "Silver" : "Gold");
    }

    void PlaceKeys()
    {
        HashSet<Vector2Int> used = new HashSet<Vector2Int>();
        for (int i = 0; i < lockedExits.Count; i++)
        {
            Vector2Int sourcePos = lockedExits[i].pos + GetDirectionVector(lockedExits[i].direction);
            if (sourcePos == Vector2Int.zero || used.Contains(sourcePos)) continue;
            if (!placedRooms.ContainsKey(sourcePos)) continue;

            GameObject prefab = null;
            switch (lockedExits[i].lockType)
            {
                case "Silver":
                    prefab = silverKeyPrefab;
                    break;
                case "Gold":
                    prefab = goldKeyPrefab;
                    break;
            }

            if (prefab == null) continue;

            Vector3 pos = placedRooms[sourcePos].transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
            Instantiate(prefab, pos, Quaternion.identity);
            used.Add(sourcePos);
        }
    }

    void PlaceItemsAndEnemies()
    {
        foreach (var kvp in placedRooms)
        {
            Vector2Int pos = kvp.Key;
            if (pos == Vector2Int.zero || pos == portalPos) continue;

            GameObject room = placedRooms[pos];
            string tag = room.tag;

            bool isSpecial = tag == "MonsterHouse" || tag == "BossRoom" || tag == "PortalRoom";
            if (!isSpecial && IsDeadEndRoom(pos))
            {
                deadEndRooms.Add(pos);
            }
        }

        PlaceGuaranteedRareItems(deadEndRooms);

        Vector2Int bossRoomPos = Vector2Int.zero;
        int totalRooms = roomPlacementOrder.Count;


        for (int i = 0; i < totalRooms; i++)
        {
            Vector2Int pos = roomPlacementOrder[i];
            if (pos == Vector2Int.zero || pos == portalPos) continue;

            GameObject room = placedRooms[pos];

            if (room.tag == ("MonsterHouse") || room.tag == "BossRoom")
                continue;

            // === COMMON ITEM PLACEMENT ===
            if (!deadEndRooms.Contains(pos))
            {
                if (commonItemPrefabs.Count > 0)
                {
                    GameObject commonItem = commonItemPrefabs[Random.Range(0, commonItemPrefabs.Count)];
                    Vector3 commonPos = GetValidSpawnPosition(room.transform.position);
                    Instantiate(commonItem, commonPos, Quaternion.identity);
                }

                int extraItems = Random.Range(0, maxItemPerRoom);
                for (int j = 0; j < extraItems; j++)
                {
                    GameObject extraItem = commonItemPrefabs[Random.Range(0, commonItemPrefabs.Count)];
                    Vector3 extraPos = GetValidSpawnPosition(room.transform.position);
                    Instantiate(extraItem, extraPos, Quaternion.identity);
                }
            }

            int maxDistance = 1;
            foreach (int dist in roomDistanceFromStart.Values)
                maxDistance = Mathf.Max(maxDistance, dist);

            int roomDist = roomDistanceFromStart.ContainsKey(pos) ? roomDistanceFromStart[pos] : 0;
            float roomProgress = (float)roomDist / maxDistance;

            int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
            // === ENEMY PLACEMENT ===
            for (int j = 0; j < enemyCount; j++)
            {
                GameObject enemyToSpawn = GetEnemyForProgress(roomProgress);
                if (enemyToSpawn == null || room.tag == "SafeRoom")
                    continue;

                Vector3 spawnPos = GetValidSpawnPosition(room.transform.position);
                Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
            }
        }
    }

    void PlaceGuaranteedRareItems(List<Vector2Int> deadEndRooms)
    {
        Debug.Log("PLACING ITEMSSSS");

        foreach (Vector2Int pos in deadEndRooms)
        {
            if (!placedRooms.ContainsKey(pos) || significantItemPrefabs.Count == 0)
                continue;

            GameObject room = placedRooms[pos];
            GameObject rareItem = significantItemPrefabs[Random.Range(0, significantItemPrefabs.Count)];
            Vector3 rarePos = room.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
            Instantiate(rareItem, rarePos, Quaternion.identity);
            Debug.Log($"[Rare Reward] GUARANTEED rare item placed in dead-end room {room.name}");
        }
    }


    Vector2Int GetDirectionVector(string dir)
    {
        if (dir == "Top") return Vector2Int.up;
        if (dir == "Right") return Vector2Int.right;
        if (dir == "Bottom") return Vector2Int.down;
        if (dir == "Left") return Vector2Int.left;
        return Vector2Int.zero;
    }

    Vector2Int FindFarthestAdjacentRoom(Vector2Int bossRoom)
    {
        float maxDist = -1f;
        Vector2Int candidate = Vector2Int.zero;
        for (int i = 0; i < roomPlacementOrder.Count; i++)
        {
            Vector2Int pos = roomPlacementOrder[i];
            if (pos == bossRoom || pos == Vector2Int.zero) continue;

            float toBoss = Vector2Int.Distance(pos, bossRoom);
            float toStart = Vector2Int.Distance(pos, Vector2Int.zero);

            if (toBoss <= 2 && toStart > maxDist)
            {
                maxDist = toStart;
                candidate = pos;
            }
        }

        return candidate;
    }

    void OnDrawGizmos()
    {
        if (placedRooms == null || placedRooms.Count == 0) return;

        List<Collider2D> allColliders = new List<Collider2D>();

        // Collect all overlay colliders from placed rooms
        foreach (var room in placedRooms.Values)
        {
            var overlay = room.transform.Find("RoomOverlayCollider");
            if (!overlay) continue;

            var colliders = overlay.GetComponentsInChildren<BoxCollider2D>();
            allColliders.AddRange(colliders);
        }

        // Check for overlaps using reduced-size bounds (0.9x strictness)
        for (int i = 0; i < allColliders.Count; i++)
        {
            bool isOverlapping = false;

            for (int j = 0; j < allColliders.Count; j++)
            {
                if (i == j) continue;

                Bounds a = allColliders[i].bounds;
                Bounds b = allColliders[j].bounds;

                // Shrink each collider bounds by 10% per axis (so overlap must be more significant)
                a.Expand(new Vector3(-overlayStrictPercentReduction * a.size.x, -overlayStrictPercentReduction * a.size.y, 0));
                b.Expand(new Vector3(-overlayStrictPercentReduction * b.size.x, -overlayStrictPercentReduction * b.size.y, 0));

                if (a.Intersects(b))
                {
                    isOverlapping = true;
                    break;
                }
            }

            Gizmos.color = isOverlapping ? Color.red : Color.green;
            Gizmos.DrawWireCube(allColliders[i].bounds.center, allColliders[i].bounds.size);
        }
    }

    bool IsRoomOverlapping(GameObject room)
    {
        var newOverlay = room.transform.Find("RoomOverlayCollider");
        if (newOverlay == null) return false;

        var newColliders = newOverlay.GetComponentsInChildren<BoxCollider2D>();

        foreach (var otherRoom in placedRooms.Values)
        {
            if (otherRoom == room) continue;

            var otherOverlay = otherRoom.transform.Find("RoomOverlayCollider");
            if (otherOverlay == null) continue;

            var otherColliders = otherOverlay.GetComponentsInChildren<BoxCollider2D>();

            foreach (var a in newColliders)
            {
                foreach (var b in otherColliders)
                {
                    Bounds boundsA = a.bounds;
                    Bounds boundsB = b.bounds;

                    boundsA.Expand(new Vector3(-overlayStrictPercentReduction * boundsA.size.x,
                                               -overlayStrictPercentReduction * boundsA.size.y, 0));
                    boundsB.Expand(new Vector3(-overlayStrictPercentReduction * boundsB.size.x,
                                               -overlayStrictPercentReduction * boundsB.size.y, 0));

                    if (boundsA.Intersects(boundsB))
                        return true;
                }
            }
        }

        return false;
    }
    bool WouldOverlapAtPosition(GameObject prefab, Vector3 targetPosition)
    {
        Transform overlay = prefab.transform.Find("RoomOverlayCollider");
        if (!overlay)
        {
            Debug.LogWarning("Missing overlay in prefab: " + prefab.name);
            return false;
        }

        BoxCollider2D[] testBoxes = overlay.GetComponentsInChildren<BoxCollider2D>();
        List<Bounds> simulatedBounds = new List<Bounds>();

        foreach (BoxCollider2D box in testBoxes)
        {
            // Calculate the simulated world position of this box
            Vector3 localOffset = box.transform.localPosition;
            Vector3 simulatedPos = targetPosition + localOffset;

            Bounds bounds = new Bounds(simulatedPos, box.size);
            bounds.Expand(new Vector3(
                -overlayStrictPercentReduction * bounds.size.x,
                -overlayStrictPercentReduction * bounds.size.y,
                0));
            simulatedBounds.Add(bounds);
        }

        foreach (GameObject placed in placedRooms.Values)
        {
            Transform placedOverlay = placed.transform.Find("RoomOverlayCollider");
            if (!placedOverlay) continue;

            BoxCollider2D[] placedBoxes = placedOverlay.GetComponentsInChildren<BoxCollider2D>();
            foreach (BoxCollider2D placedBox in placedBoxes)
            {
                Bounds placedBounds = placedBox.bounds;
                placedBounds.Expand(new Vector3(
                    -overlayStrictPercentReduction * placedBounds.size.x,
                    -overlayStrictPercentReduction * placedBounds.size.y,
                    0));

                foreach (Bounds testBound in simulatedBounds)
                {
                    if (testBound.Intersects(placedBounds))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void FinalizeRoomConnections()
    {
        foreach (var kvp in placedRooms)
        {
            Vector2Int pos = kvp.Key;
            GameObject room = kvp.Value;

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int dir = directions[i];
                Vector2Int neighborPos = pos + dir;

                if (placedRooms.ContainsKey(neighborPos))
                {
                    string side = directionNames[i];
                    string opposite = directionNames[(i + 2) % 4]; // Top-Bottom, Right-Left pairs

                    DisableWall(pos, side);
                    DisableWall(neighborPos, opposite);
                }
            }
        }
    }
    Vector2Int GetGridPosFromRoom(GameObject room)
    {
        foreach (var kvp in placedRooms)
        {
            if (kvp.Value == room)
                return kvp.Key;
        }
        Debug.LogWarning($"Room {room.name} not found in placedRooms!");
        return Vector2Int.zero;
    }

    #region DebugSpecial
    IEnumerator GenerateBranch()
    {
        if (frontier.Count == 0)
        {
            Debug.LogWarning("Frontier empty, cannot generate branch.");
            yield break;
        }

        // Pick random frontier position as branch start
        Vector2Int start = frontier[Random.Range(0, frontier.Count)];
        Vector2Int current = start;

        // Randomly decide branch length
        int branchLength = Random.Range(3, 6);

        for (int i = 0; i < branchLength; i++)
        {
            // Random direction (prefer directions with space)
            List<string> possibleDirs = new List<string> { "Top", "Right", "Bottom", "Left" };
            string fromDir = possibleDirs[Random.Range(0, possibleDirs.Count)];
            Vector2Int dirVector = GetDirectionVector(fromDir);
            Vector2Int next = current + dirVector;

            if (placedRooms.ContainsKey(next))
            {
                Debug.Log($"[Branch] Position {next} already occupied, skipping step {i}");
                continue;
            }

            string incomingDir = GetOppositeDirection(fromDir);
            RoomData fromData = placedRooms[current].GetComponent<RoomData>();
            GameObject newRoom = PlaceRoom(next, incomingDir, fromData, fromDir);

            if (newRoom == null)
            {
                Debug.Log($"[Branch] Failed to place room at {next}");
                continue;
            }

            newRoom.name = $"BranchRoom_{i}";
            newRoom.tag = "BranchStructure";

            DisableWall(current, fromDir);
            DisableWall(next, incomingDir);
            EnableDoor(current, fromDir, "");
            EnableDoor(next, incomingDir, "");

            placedRooms[next] = newRoom;
            roomPlacementOrder.Add(next);
            frontier.Add(next);

            current = next;
            roomsPlaced++;

            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("Branch generation complete");
    }
    IEnumerator GenerateForkStructure(Vector2Int origin)
    {
        Vector2Int[] forkDirs = new Vector2Int[] { Vector2Int.up, Vector2Int.right };
        string[] dirNames = new string[] { "Top", "Right" };
        int pathLength = 3;

        // Reference from origin
        RoomData fromData = placedRooms[origin].GetComponent<RoomData>();

        // Store both final ends
        Vector2Int portalEnd = origin;
        Vector2Int keyEnd = origin;

        for (int path = 0; path < 2; path++)
        {
            Vector2Int current = origin;
            Vector2Int dir = forkDirs[path];
            string dirName = dirNames[path];
            string oppositeDir = GetOppositeDirection(dirName);

            for (int step = 0; step < pathLength; step++)
            {
                Vector2Int next = current + dir;
                if (placedRooms.ContainsKey(next)) break; // Avoid overwriting existing room

                GameObject newRoom = PlaceRoom(next, GetOppositeDirection(dirName), placedRooms[current].GetComponent<RoomData>(), dirName);
                if (newRoom == null) break;

                DisableWall(current, dirName);
                DisableWall(next, GetOppositeDirection(dirName));
                EnableDoor(current, dirName, "");
                EnableDoor(next, GetOppositeDirection(dirName), "");

                newRoom.name = $"Fork_{(path == 0 ? "Portal" : "Key")}_{step}";
                roomPlacementOrder.Add(next);
                placedRooms[next] = newRoom;
                frontier.Add(next);

                current = next;
                yield return new WaitForSeconds(0.05f);
            }

            // Set final room
            if (path == 0)
                portalEnd = current;
            else
                keyEnd = current;
        }

        //// Place portal
        //if (placedRooms.ContainsKey(portalEnd))
        //{
        //    Instantiate(portalPrefab, placedRooms[portalEnd].transform.position, Quaternion.identity);
        //    Debug.Log($"Portal placed at {portalEnd}");
        //}

        //// Place purple key
        //if (placedRooms.ContainsKey(keyEnd))
        //{
        //    Vector3 dropPos = placedRooms[keyEnd].transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
        //    Instantiate(purpleKeyPrefab, dropPos, Quaternion.identity);
        //    Debug.Log($"Purple Key placed at {keyEnd}");
        //}
    }
    IEnumerator GenerateLoop()
    {
        if (frontier.Count == 0)
        {
            Debug.LogWarning("Frontier empty, cannot generate loop.");
            yield break;
        }

        // Pick random frontier position as loop start
        Vector2Int start = frontier[Random.Range(0, frontier.Count)];
        Vector2Int current = start;

        string[] pathSequence = new string[]
        {
        "Top", "Top", "Right", "Right", "Bottom", "Left", "Left"
        };

        for (int i = 0; i < pathSequence.Length; i++)
        {
            string fromDir = pathSequence[i];
            Vector2Int dirVector = GetDirectionVector(fromDir);
            Vector2Int next = current + dirVector;

            if (placedRooms.ContainsKey(next)) continue;

            string incomingDir = GetOppositeDirection(fromDir);
            RoomData fromData = placedRooms[current].GetComponent<RoomData>();
            GameObject newRoom = PlaceSquareRoom(next, squarePrefab, incomingDir, fromData, fromDir);

            if (newRoom == null)
            {
                Debug.Log($"[Loop] Failed to place room at {next}");
                continue;
            }

            newRoom.name = $"LoopRoom_{i}";
            newRoom.tag = "LoopStructure";

            DisableWall(current, fromDir);
            DisableWall(next, incomingDir);
            EnableDoor(current, fromDir, "");
            EnableDoor(next, incomingDir, "");

            placedRooms[next] = newRoom;
            roomPlacementOrder.Add(next);
            frontier.Add(next);

            current = next;
            roomsPlaced++;
            yield return new WaitForSeconds(0.1f);
        }

        // Connect last to first room in loop
        GameObject firstRoom = GameObject.Find("LoopRoom_0");

        GameObject lastRoom = GameObject.Find("LoopRoom_5");
        if (lastRoom == null)
        {
            Debug.Log("Cannot find room 5");
            lastRoom = GameObject.Find("LoopRoom_6");
        }


        if (firstRoom != null && lastRoom != null)
        {
            if (lastRoom.name == "LoopRoom_5")
            {
                DisableWall(firstRoom, "Right");
                DisableWall(lastRoom, "Left");
                EnableDoor(firstRoom, "Right", "");
                EnableDoor(lastRoom, "Left", "");
            }
            else if (lastRoom.name == "LoopRoom_5")
            {
                if (firstRoom != null && lastRoom != null)
                {
                    Vector2Int firstGridPos = Vector2Int.RoundToInt((Vector2)firstRoom.transform.position / distanceOffset);
                    Vector2Int lastGridPos = Vector2Int.RoundToInt((Vector2)lastRoom.transform.position / distanceOffset);

                    DisableWall(firstGridPos, "Right");
                    DisableWall(lastGridPos, "Left");
                    EnableDoor(firstGridPos, "Right", "");
                    EnableDoor(lastGridPos, "Left", "");
                }
            }



            Debug.Log("Hard-coded opened Right exit of firstRoom and Left exit of lastRoom");
        }


        Debug.Log("Loop generation complete (anchored to random frontier).");
    }
    IEnumerator GenerateSafeRoom(Vector2Int origin)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int dir = directions[i];
            Vector2Int next = origin + dir;

            if (placedRooms.ContainsKey(next)) continue;

            string fromDir = directionNames[i];
            string incomingDir = GetOppositeDirection(fromDir);

            RoomData fromData = placedRooms[origin].GetComponent<RoomData>();
            GameObject room = TryPlaceRoom(next, safeRoomPrefab, incomingDir, fromData, fromDir);

            if (room == null) continue;

            DisableWall(origin, fromDir);
            DisableWall(next, incomingDir);
            EnableDoor(origin, fromDir, "");
            EnableDoor(next, incomingDir, "");

            room.name = "SafeRoom";
            room.tag = "SafeRoom";
            placedRooms[next] = room;
            roomPlacementOrder.Add(next);
            frontier.Add(next);
            roomsPlaced++;

            Debug.Log($"[SafeRoom] Placed safe room at {next}");
            break;
        }

        yield return null;
    }

    #endregion

    #region ReleaseSpecial
    void GenerateLoopSync()
    {
        if (frontier.Count == 0)
        {
            Debug.LogWarning("Frontier empty, cannot generate loop.");
            return;
        }

        Vector2Int start = frontier[Random.Range(0, frontier.Count)];
        Vector2Int current = start;

        string[] pathSequence = new string[]
        {
        "Top", "Top", "Right", "Right", "Bottom", "Left", "Left"
        };

        for (int i = 0; i < pathSequence.Length; i++)
        {
            string fromDir = pathSequence[i];
            Vector2Int dirVector = GetDirectionVector(fromDir);
            Vector2Int next = current + dirVector;

            if (placedRooms.ContainsKey(next)) continue;

            string incomingDir = GetOppositeDirection(fromDir);
            RoomData fromData = placedRooms[current].GetComponent<RoomData>();
            GameObject newRoom = PlaceSquareRoom(next, squarePrefab, incomingDir, fromData, fromDir);

            if (newRoom == null)
            {
                Debug.Log($"[Loop] Failed to place room at {next}");
                continue;
            }

            newRoom.name = $"LoopRoom_{i}";
            newRoom.tag = "LoopStructure";

            DisableWall(current, fromDir);
            DisableWall(next, incomingDir);
            EnableDoor(current, fromDir, "");
            EnableDoor(next, incomingDir, "");

            placedRooms[next] = newRoom;
            roomPlacementOrder.Add(next);
            frontier.Add(next);

            current = next;
            roomsPlaced++;
        }

        GameObject firstRoom = GameObject.Find("LoopRoom_0");
        GameObject lastRoom = GameObject.Find("LoopRoom_5") ?? GameObject.Find("LoopRoom_6");

        if (firstRoom != null && lastRoom != null)
        {
            Vector2Int firstGridPos = GetGridPosFromRoom(firstRoom);
            Vector2Int lastGridPos = GetGridPosFromRoom(lastRoom);

            DisableWall(firstGridPos, "Right");
            DisableWall(lastGridPos, "Left");
            EnableDoor(firstGridPos, "Right", "");
            EnableDoor(lastGridPos, "Left", "");
            Debug.Log("Hard-coded opened Right exit of firstRoom and Left exit of lastRoom");
            Debug.Log("First Room name: " + firstRoom.name);
            Debug.Log("Last Room name: " + lastRoom.name);

        }

        Debug.Log("Loop generation complete.");
    }
    void GenerateBranchSync()
    {
        if (frontier.Count == 0)
        {
            Debug.LogWarning("Frontier empty, cannot generate branch.");
            return;
        }

        Vector2Int start = frontier[Random.Range(0, frontier.Count)];
        Vector2Int current = start;
        int branchLength = Random.Range(3, 6);

        for (int i = 0; i < branchLength; i++)
        {
            string[] possibleDirs = { "Top", "Right", "Bottom", "Left" };
            string fromDir = possibleDirs[Random.Range(0, possibleDirs.Length)];
            Vector2Int dirVector = GetDirectionVector(fromDir);
            Vector2Int next = current + dirVector;

            if (placedRooms.ContainsKey(next))
            {
                Debug.Log($"[Branch] Position {next} already occupied, skipping step {i}");
                continue;
            }

            string incomingDir = GetOppositeDirection(fromDir);
            RoomData fromData = placedRooms[current].GetComponent<RoomData>();
            GameObject newRoom = PlaceRoom(next, incomingDir, fromData, fromDir);

            if (newRoom == null)
            {
                Debug.Log($"[Branch] Failed to place room at {next}");
                continue;
            }

            newRoom.name = $"BranchRoom_{i}";
            newRoom.tag = "BranchStructure";

            DisableWall(current, fromDir);
            DisableWall(next, incomingDir);
            EnableDoor(current, fromDir, "");
            EnableDoor(next, incomingDir, "");

            placedRooms[next] = newRoom;
            roomPlacementOrder.Add(next);
            frontier.Add(next);

            current = next;
            roomsPlaced++;
        }

        Debug.Log("Branch generation complete.");
    }
    void GenerateForkStructureSync(Vector2Int origin)
    {
        Vector2Int[] forkDirs = new Vector2Int[] { Vector2Int.up, Vector2Int.right };
        string[] dirNames = new string[] { "Top", "Right" };
        int pathLength = 3;

        Vector2Int portalEnd = origin;
        Vector2Int keyEnd = origin;

        for (int path = 0; path < 2; path++)
        {
            Vector2Int current = origin;
            Vector2Int dir = forkDirs[path];
            string dirName = dirNames[path];
            string oppositeDir = GetOppositeDirection(dirName);

            for (int step = 0; step < pathLength; step++)
            {
                Vector2Int next = current + dir;
                if (placedRooms.ContainsKey(next)) break;

                GameObject newRoom = PlaceRoom(next, GetOppositeDirection(dirName), placedRooms[current].GetComponent<RoomData>(), dirName);
                if (newRoom == null) break;

                DisableWall(current, dirName);
                DisableWall(next, GetOppositeDirection(dirName));
                EnableDoor(current, dirName, "");
                EnableDoor(next, GetOppositeDirection(dirName), "");

                newRoom.name = $"Fork_{(path == 0 ? "Portal" : "Key")}_{step}";
                roomPlacementOrder.Add(next);
                placedRooms[next] = newRoom;
                frontier.Add(next);

                current = next;
                roomsPlaced++;
            }

            if (path == 0)
                portalEnd = current;
            else
                keyEnd = current;
        }

        // Purple key now handled elsewhere, not placed here to avoid duplication
        Debug.Log("Fork structure generated.");
    }
    bool GenerateSafeRoomSync(Vector2Int origin)
    {
        if (safeRoomPlaced) return true;

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int dir = directions[i];
            Vector2Int next = origin + dir;

            if (placedRooms.ContainsKey(next)) continue;

            string fromDir = directionNames[i];
            string incomingDir = GetOppositeDirection(fromDir);
            RoomData fromData = placedRooms[origin].GetComponent<RoomData>();

            GameObject room = TryPlaceRoom(next, safeRoomPrefab, incomingDir, fromData, fromDir);

            if (room == null) continue;

            DisableWall(origin, fromDir);
            DisableWall(next, incomingDir);
            EnableDoor(origin, fromDir, "");
            EnableDoor(next, incomingDir, "");

            room.name = "SafeRoom";
            room.tag = "SafeRoom";
            placedRooms[next] = room;
            roomPlacementOrder.Add(next);
            frontier.Add(next);
            roomsPlaced++;

            safeRoomPlaced = true;
            Debug.Log($"[SafeRoom] Placed safe room at {next}");
            return true;
        }

        return false;
    }

    #endregion
    void EnablePurpleDoor(Vector2Int pos, string dirName)
    {
        Transform pd = placedRooms[pos].transform.Find(dirName + "PurpleDoor");
        if (pd) pd.gameObject.SetActive(true);
    }

    string Opposite(string dir)
    {
        return dir == "Top" ? "Bottom"
             : dir == "Bottom" ? "Top"
             : dir == "Left" ? "Right"
             : "Left";
    }

    GameObject GetEnemyForProgress(float roomProgress)
    {
        if (enemyBucket == null)
        {
            Debug.Log("Bucket where");
            return null;
        }
        if (playerRecords == null)
        {
            Debug.Log("PlayerRecordWhere");
            return null;
        }
        int deaths = playerRecords.GetPlayerDeathCount();
        int clears = playerRecords.GetPlayerLevelClear();

        float difficultyFactor = Mathf.Clamp01(0.5f + 0.05f * (clears - deaths));

        // Interpret difficultyFactor as:
        // < 0.3  → mostly trivial/easy
        // 0.3–0.6 → normal scaling
        // > 0.6  → ramp into medium/hard earlier

        List<GameObject> bucket;

        if (roomProgress < 0.2f)
        {
            bucket = (difficultyFactor < 0.4f)
                ? enemyBucket.trivialEnemies
                : enemyBucket.easyEnemies;
        }
        else if (roomProgress < 0.45f)
        {
            if (difficultyFactor < 0.3f)
                bucket = Random.value < 0.7f ? enemyBucket.trivialEnemies : enemyBucket.easyEnemies;
            else
                bucket = Random.value < 0.5f ? enemyBucket.easyEnemies : enemyBucket.mediumEnemies;
        }
        else if (roomProgress < 0.75f)
        {
            if (difficultyFactor < 0.3f)
                bucket = enemyBucket.easyEnemies;
            else if (difficultyFactor < 0.7f)
                bucket = Random.value < 0.5f ? enemyBucket.easyEnemies : enemyBucket.mediumEnemies;
            else
                bucket = Random.value < 0.5f ? enemyBucket.mediumEnemies : enemyBucket.hardEnemies;
        }
        else
        {
            bucket = (difficultyFactor > 0.4f)
                ? (Random.value < 0.6f ? enemyBucket.hardEnemies : enemyBucket.mediumEnemies)
                : enemyBucket.mediumEnemies;
        }

        if (roomProgress > 0.8f && difficultyFactor > 0.4f)
        {
            bucket = enemyBucket.hardEnemies;
        }



        if (bucket == null || bucket.Count == 0)
            return null;

        return bucket[Random.Range(0, bucket.Count)];
    }

    GameObject TryPlaceRoom(Vector2Int gridPos, GameObject roomPrefab, string incomingDir, RoomData fromRoom, string fromDir)
    {
        Vector3 targetPosition;

        if (incomingDir != null && fromRoom != null && fromDir != null)
        {
            RoomData tempData = roomPrefab.GetComponent<RoomData>();
            Transform thisAnchor = tempData.GetAnchor(incomingDir);
            Transform fromAnchor = fromRoom.GetAnchor(fromDir);

            if (thisAnchor && fromAnchor)
            {
                targetPosition = fromAnchor.position - thisAnchor.localPosition;
            }
            else
            {
                targetPosition = new Vector3(gridPos.x * distanceOffset, gridPos.y * distanceOffset, 0);
            }
        }
        else
        {
            targetPosition = new Vector3(gridPos.x * distanceOffset, gridPos.y * distanceOffset, 0);
        }

        if (WouldOverlapAtPosition(roomPrefab, targetPosition))
        {
            return null;
        }

        GameObject room = Instantiate(roomPrefab, targetPosition, Quaternion.identity);
        placedRooms[gridPos] = room;

        RoomData data = room.GetComponent<RoomData>();
        if (data == null)
        {
            Debug.LogError("Missing RoomData on prefab");
            return room;
        }

        for (int i = 0; i < 4; i++)
        {
            string dir = directionNames[i];
            Transform wall = room.transform.Find(dir + "Exit");
            if (wall) wall.gameObject.SetActive(true);
            Transform silverDoor = room.transform.Find(dir + "SilverDoor");
            if (silverDoor) silverDoor.gameObject.SetActive(false);
            Transform goldDoor = room.transform.Find(dir + "GoldDoor");
            if (goldDoor) goldDoor.gameObject.SetActive(false);
            Transform purpleDoor = room.transform.Find(dir + "PurpleDoor");
            if (purpleDoor) purpleDoor.gameObject.SetActive(false);
        }

        return room;
    }
    bool IsDeadEndRoom(Vector2Int pos)
    {
        if (!placedRooms.ContainsKey(pos)) return false;



        GameObject room = placedRooms[pos];
        int openWallCount = 0;

        foreach (string dir in directionNames)
        {
            //Debug.Log("WallName: " + dir + "Exit"); 
            Transform wall = room.transform.Find(dir + "Exit");

            if (wall != null && !wall.gameObject.activeSelf)
            {
                openWallCount++;
            }
        }
        if (openWallCount == 1)
        {
            Debug.Log(room.name + " is a dead end");
        }
        return openWallCount == 1;
    }

    bool ForcePlaceBossRoom()
    {
        foreach (Vector2Int frontierRoom in frontier)
        {
            if (Vector2Int.Distance(frontierRoom, Vector2Int.zero) < 4f)
                continue; // Skip rooms too close to start

            GameObject baseRoom = placedRooms[frontierRoom];
            RoomData baseData = baseRoom.GetComponent<RoomData>();

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int dir = directions[i];
                Vector2Int newPos = frontierRoom + dir;

                // Skip if this spot is taken
                if (placedRooms.ContainsKey(newPos)) continue;

                string fromDir = directionNames[i];
                string incomingDir = GetOppositeDirection(fromDir);

                GameObject chosenBossRoom = bossRoomPrefabs[Random.Range(0, bossRoomPrefabs.Count)];
                GameObject bossRoom = TryPlaceRoom(newPos, chosenBossRoom, incomingDir, baseData, fromDir);

                if (bossRoom != null)
                {
                    DisableWall(frontierRoom, fromDir);
                    DisableWall(newPos, incomingDir);
                    EnableDoor(frontierRoom, fromDir, "");
                    EnableDoor(newPos, incomingDir, "");

                    bossRoom.name = "BossRoom";
                    bossRoom.tag = "BossRoom";
                    bossRoomPos = newPos;
                    bossRoomPlaced = true;

                    placedRooms[newPos] = bossRoom;
                    roomPlacementOrder.Add(newPos);
                    frontier.Add(newPos);
                    roomsPlaced++;

                    Debug.Log($"[BossRoom] Forced placement succeeded at {newPos}.");
                    return true;
                }
            }
        }

        Debug.LogWarning("[BossRoom] Forced placement failed after trying all outer anchors.");
        return false;
    }

    void ComputeRoomDistancesFromStart()
    {
        roomDistanceFromStart.Clear();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(Vector2Int.zero);
        roomDistanceFromStart[Vector2Int.zero] = 0;
        visited.Add(Vector2Int.zero);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDist = roomDistanceFromStart[current];

            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (placedRooms.ContainsKey(neighbor) && !visited.Contains(neighbor))
                {
                    roomDistanceFromStart[neighbor] = currentDist + 1;
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
    }

    bool IsPositionClear(Vector3 position, float radius = 0.4f, LayerMask? layerMask = null)
    {
        if (layerMask.HasValue)
            return Physics2D.OverlapCircle(position, radius, layerMask.Value) == null;

        return Physics2D.OverlapCircle(position, radius) == null;
    }

    Vector3 GetValidSpawnPosition(Vector3 roomCenter, int maxAttempts = 10, float checkRadius = 0.4f)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-1.5f, 1.5f),
                Random.Range(-1.5f, 1.5f),
                0f
            );

            Vector3 potentialPosition = roomCenter + randomOffset;

            if (IsPositionClear(potentialPosition, checkRadius))
            {
                return potentialPosition;
            }
        }

        return roomCenter + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);
    }

    GameObject GetValidStartRoomPrefab()
    {
        foreach (GameObject prefab in roomPrefabs)
        {
            if (prefab.tag != "GoblinHouse")
            {
                return prefab;
            }
        }

        Debug.LogError("No valid start room prefab found (non-GoblinHouse). Using squarePrefab as fallback.");
        return squarePrefab;
    }

}