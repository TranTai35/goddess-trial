#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Scene1ForestRuinsGenerator
{
    private const string Base = "Assets/MapAndCharacter/Packed_RPGTinyHeroWorldBundlePBR/RPG Tiny Fantasy World 01 PBR/Prefab/";
    private const string OutputScene = "Assets/Scenes/Scene1_ForestRuins.unity";

    [MenuItem("Tools/Goddess Trial/Generate Scene 1 - Forest Ruins")]
    public static void Generate()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject map = NewRoot("SCENE_1_FOREST_RUINS");
        GameObject environment = NewChild(map, "Environment");
        GameObject gameplay = NewChild(map, "Gameplay_Markers");
        GameObject lighting = NewChild(map, "Lighting");

        GameObject land = NewChild(environment, "01_LandMass");
        GameObject roads = NewChild(environment, "02_Roads");
        GameObject rocks = NewChild(environment, "03_Rocks_Gameplay");
        GameObject trees = NewChild(environment, "04_Trees");
        GameObject ruins = NewChild(environment, "05_Ruins");
        GameObject background = NewChild(environment, "06_Background_Mountains");
        GameObject details = NewChild(environment, "07_Decoration");

        // Five readable gameplay zones arranged in a gentle S curve.
        Place("LandMass/LM40RND.prefab", land.transform, "Land_Start", new Vector3(0, 0, 0), Vector3.zero, Vector3.one);
        Place("LandMass/LM40RND.prefab", land.transform, "Land_Arena01", new Vector3(8, 0, 25), Vector3.zero, Vector3.one);
        Place("LandMass/LM40RND.prefab", land.transform, "Land_ForestFork", new Vector3(-4, 0, 50), Vector3.zero, Vector3.one);
        Place("LandMass/LM40RND.prefab", land.transform, "Land_Arena02", new Vector3(-12, 0, 76), Vector3.zero, new Vector3(1.15f,1,1.05f));
        Place("LandMass/LM40RND.prefab", land.transform, "Land_BossArena", new Vector3(0, 0, 106), Vector3.zero, new Vector3(1.25f,1,1.15f));

        // Smaller pieces soften joins and create the side reward branch.
        Place("LandMass/LM20RND.prefab", land.transform, "Land_Link01", new Vector3(5, 0, 13), Vector3.zero, Vector3.one);
        Place("LandMass/LM20RND.prefab", land.transform, "Land_Link02", new Vector3(2, 0, 38), Vector3.zero, Vector3.one);
        Place("LandMass/LM20RND.prefab", land.transform, "Land_Link03", new Vector3(-9, 0, 63), Vector3.zero, Vector3.one);
        Place("LandMass/LM20RND.prefab", land.transform, "Land_Link04", new Vector3(-7, 0, 91), Vector3.zero, Vector3.one);
        Place("LandMass/LM20RND.prefab", land.transform, "Land_RewardBranch", new Vector3(18, 0, 52), Vector3.zero, Vector3.one);

        // Dirt/stone guidance. Exact art can be swapped without changing layout.
        Vector3[] roadPoints = {
            new Vector3(0,0.08f,5), new Vector3(3,0.08f,15), new Vector3(8,0.08f,25),
            new Vector3(3,0.08f,38), new Vector3(-4,0.08f,50), new Vector3(-9,0.08f,63),
            new Vector3(-12,0.08f,76), new Vector3(-7,0.08f,91), new Vector3(0,0.08f,103)
        };
        for (int i = 0; i < roadPoints.Length; i++)
            Place(i % 2 == 0 ? "RiverRoadLakeFall/RoadA01.prefab" : "RiverRoadLakeFall/RoadC01.prefab",
                roads.transform, $"Road_{i+1:00}", roadPoints[i], new Vector3(0, (i * 37) % 360, 0), Vector3.one);

        Place("BuildingUtilityDeco/Portal01.prefab", ruins.transform, "Broken_Entrance_Portal", new Vector3(0,0.3f,-7), Vector3.zero, Vector3.one);
        Place("BuildingUtilityDeco/SignPost01.prefab", details.transform, "Direction_Sign", new Vector3(-3,0.2f,8), new Vector3(0,35,0), Vector3.one);

        // Arena 1: open and beginner-friendly.
        RingRocks(rocks.transform, new Vector3(8,0,25), 13f, 7, 20f, 150f, 2);
        PlaceTrees(trees.transform, new Vector3(8,0,25), 15f, 10, 101, 160f, 350f);

        // Fork and optional reward path.
        Place("BuildingUtilityDeco/Bridge02.prefab", ruins.transform, "Reward_Path_Bridge", new Vector3(10,0.15f,51), new Vector3(0,90,0), Vector3.one);
        Place("BuildingUtilityDeco/Crystal01.prefab", details.transform, "Reward_Crystal", new Vector3(20,0.4f,53), Vector3.zero, new Vector3(1.25f,1.25f,1.25f));
        Place("BuildingUtilityDeco/WoodBarrel01.prefab", details.transform, "Reward_Barrel", new Vector3(17,0.2f,56), new Vector3(0,25,0), Vector3.one);
        PlaceTrees(trees.transform, new Vector3(-4,0,50), 16f, 13, 202, 15f, 345f);

        // Arena 2: central cover and ranged-enemy positions.
        Place("Rock/RockCliff03.prefab", rocks.transform, "Arena02_CentralCover", new Vector3(-12,0.3f,76), new Vector3(0,35,0), new Vector3(1.4f,1.2f,1.4f));
        Place("Rock/Rock06.prefab", rocks.transform, "Arena02_LeftCover", new Vector3(-22,0.2f,75), new Vector3(0,80,0), new Vector3(1.3f,1.3f,1.3f));
        Place("Rock/Rock08.prefab", rocks.transform, "Arena02_RightCover", new Vector3(-2,0.2f,81), new Vector3(0,20,0), new Vector3(1.3f,1.3f,1.3f));
        Place("BuildingUtilityDeco/Pillar01.prefab", ruins.transform, "Ranged_Perch_Left", new Vector3(-23,0.2f,84), Vector3.zero, Vector3.one);
        Place("BuildingUtilityDeco/Pillar02.prefab", ruins.transform, "Ranged_Perch_Right", new Vector3(-2,0.2f,70), Vector3.zero, Vector3.one);
        RingRocks(rocks.transform, new Vector3(-12,0,76), 16f, 8, 30f, 150f, 6);
        PlaceTrees(trees.transform, new Vector3(-12,0,76), 18f, 13, 303, 160f, 345f);

        // Boss ruins: strong silhouette at the end of the map.
        Place("BuildingUtilityDeco/Gate04.prefab", ruins.transform, "Boss_Gate", new Vector3(0,0.2f,121), new Vector3(0,180,0), new Vector3(1.4f,1.4f,1.4f));
        Place("BuildingUtilityDeco/Portal03.prefab", ruins.transform, "Exit_Portal", new Vector3(0,0.35f,125), new Vector3(0,180,0), Vector3.one);
        Place("BuildingUtilityDeco/Wall03.prefab", ruins.transform, "Boss_Wall_Left", new Vector3(-10,0.2f,117), new Vector3(0,15,0), new Vector3(1.4f,1.2f,1.4f));
        Place("BuildingUtilityDeco/Wall04.prefab", ruins.transform, "Boss_Wall_Right", new Vector3(10,0.2f,117), new Vector3(0,-15,0), new Vector3(1.4f,1.2f,1.4f));
        for (int i = 0; i < 4; i++) {
            float x = i < 2 ? -12 : 12;
            float z = i % 2 == 0 ? 98 : 113;
            Place($"BuildingUtilityDeco/Pillar0{(i%4)+1}.prefab", ruins.transform, $"Boss_Pillar_{i+1}", new Vector3(x,0.2f,z), Vector3.zero, new Vector3(1.2f,1.2f,1.2f));
        }
        RingRocks(rocks.transform, new Vector3(0,0,106), 20f, 10, 25f, 155f, 10);
        PlaceTrees(trees.transform, new Vector3(0,0,106), 22f, 14, 404, 155f, 350f);

        // Background mountains frame camera view and naturally block map edges.
        Vector3[] mountainPositions = {
            new Vector3(-32,0,10), new Vector3(34,0,20), new Vector3(-38,0,48),
            new Vector3(34,0,62), new Vector3(-40,0,91), new Vector3(38,0,105),
            new Vector3(-27,0,130), new Vector3(28,0,132)
        };
        for (int i = 0; i < mountainPositions.Length; i++)
            Place($"Mountains/Mountain0{(i%7)+1}.prefab", background.transform, $"Mountain_{i+1:00}", mountainPositions[i], new Vector3(0,(i*43)%360,0), new Vector3(1.4f,1.4f,1.4f));

        // Gameplay markers are deliberately simple and easy to replace with current systems.
        Marker(gameplay.transform, "PlayerSpawn", new Vector3(0,0.5f,0), new Color(0.2f,0.8f,1f,0.75f), new Vector3(1.2f,1,1.2f));
        Marker(gameplay.transform, "Arena01_Center", new Vector3(8,0.25f,25), new Color(1f,0.8f,0.15f,0.55f), new Vector3(4,0.35f,4));
        Marker(gameplay.transform, "Arena02_Center", new Vector3(-12,0.25f,76), new Color(1f,0.55f,0.1f,0.55f), new Vector3(4,0.35f,4));
        Marker(gameplay.transform, "BossArena_Center", new Vector3(0,0.25f,106), new Color(1f,0.15f,0.15f,0.55f), new Vector3(5,0.35f,5));
        CreateSpawnGroup(gameplay.transform, "Arena01_EnemySpawns", new Vector3(8,0,25), 7f, 3, 15);
        CreateSpawnGroup(gameplay.transform, "Arena02_EnemySpawns", new Vector3(-12,0,76), 10f, 5, 35);
        CreateSpawnGroup(gameplay.transform, "BossArena_EnemySpawns", new Vector3(0,0,106), 11f, 4, 45);

        // Basic preview light and isometric camera; replace with your gameplay camera when integrating.
        GameObject sun = new GameObject("Directional Light");
        sun.transform.SetParent(lighting.transform);
        sun.transform.rotation = Quaternion.Euler(48, -35, 0);
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        light.shadows = LightShadows.Soft;

        GameObject camGO = new GameObject("Scene_Preview_Camera");
        camGO.transform.SetParent(lighting.transform);
        camGO.transform.position = new Vector3(32, 32, -30);
        camGO.transform.rotation = Quaternion.Euler(42, -28, 0);
        Camera cam = camGO.AddComponent<Camera>();
        cam.orthographic = false;
        cam.fieldOfView = 48;
        cam.farClipPlane = 500;
        camGO.tag = "MainCamera";

        EditorSceneManager.SaveScene(scene, OutputScene);
        Selection.activeGameObject = map;
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(OutputScene));
        Debug.Log($"Created {OutputScene}. Bake NavMesh after integrating your player/enemy systems.");
        EditorUtility.DisplayDialog("Scene 1 created", "Created Assets/Scenes/Scene1_ForestRuins.unity\n\nThe scene contains the full environment layout and gameplay spawn markers. Replace markers with your current player/enemy prefabs, then bake NavMesh.", "OK");
    }

    private static GameObject NewRoot(string name) => new GameObject(name);
    private static GameObject NewChild(GameObject parent, string name) { var go = new GameObject(name); go.transform.SetParent(parent.transform); return go; }

    private static GameObject Place(string relativePath, Transform parent, string name, Vector3 pos, Vector3 euler, Vector3 scale)
    {
        string path = Base + relativePath;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        GameObject go;
        if (prefab != null) go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        else {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "MISSING_" + name;
            go.transform.localScale = new Vector3(2,0.5f,2);
            Debug.LogWarning("Missing prefab: " + path);
        }
        go.name = name;
        go.transform.SetParent(parent, true);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(euler);
        go.transform.localScale = scale;
        return go;
    }

    private static void RingRocks(Transform parent, Vector3 center, float radius, int count, float startDeg, float arcDeg, int seed)
    {
        System.Random rng = new System.Random(seed);
        for (int i = 0; i < count; i++) {
            float t = count == 1 ? 0 : i / (float)(count - 1);
            float a = (startDeg + t * arcDeg) * Mathf.Deg2Rad;
            Vector3 p = center + new Vector3(Mathf.Cos(a)*radius, 0.15f, Mathf.Sin(a)*radius);
            int rock = 1 + rng.Next(0, 10);
            float s = 0.9f + (float)rng.NextDouble()*0.6f;
            Place($"Rock/Rock{rock:00}.prefab", parent, $"BoundaryRock_{center.z:000}_{i+1:00}", p, new Vector3(0,rng.Next(0,360),0), new Vector3(s,s,s));
        }
    }

    private static void PlaceTrees(Transform parent, Vector3 center, float radius, int count, int seed, float startDeg, float arcDeg)
    {
        System.Random rng = new System.Random(seed);
        for (int i = 0; i < count; i++) {
            float t = count == 1 ? 0 : i / (float)(count - 1);
            float a = (startDeg + t * arcDeg + rng.Next(-8,9)) * Mathf.Deg2Rad;
            float r = radius + (float)rng.NextDouble()*4f;
            Vector3 p = center + new Vector3(Mathf.Cos(a)*r, 0.1f, Mathf.Sin(a)*r);
            int tree = 1 + rng.Next(0,5);
            float s = 0.9f + (float)rng.NextDouble()*0.35f;
            Place($"TreePlants/Tree0{tree}.prefab", parent, $"Tree_{center.z:000}_{i+1:00}", p, new Vector3(0,rng.Next(0,360),0), new Vector3(s,s,s));
        }
    }

    private static void Marker(Transform parent, string name, Vector3 pos, Color color, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        Renderer r = go.GetComponent<Renderer>();
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        m.color = color;
        r.sharedMaterial = m;
        Collider c = go.GetComponent<Collider>(); if (c != null) UnityEngine.Object.DestroyImmediate(c);
    }

    private static void CreateSpawnGroup(Transform parent, string name, Vector3 center, float radius, int count, int startAngle)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent);
        for (int i = 0; i < count; i++) {
            float a = (startAngle + i * 360f / count) * Mathf.Deg2Rad;
            Marker(group.transform, $"EnemySpawn_{i+1:00}", center + new Vector3(Mathf.Cos(a)*radius,0.2f,Mathf.Sin(a)*radius), new Color(1f,0.2f,0.2f,0.7f), new Vector3(0.55f,0.15f,0.55f));
        }
    }
}
#endif
