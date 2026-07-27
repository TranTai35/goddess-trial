#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class Scene1LargeForestRuinsGenerator
{
    private const string BasePath =
        "Assets/MapAndCharacter/Packed_RPGTinyHeroWorldBundlePBR/" +
        "RPG Tiny Fantasy World 01 PBR/Prefab/";

    private const string OutputScene =
        "Assets/Scenes/Scene1_ForestRuins_Large.unity";

    // Original dimensions of the prefabs used by the generator.
    private const float NaturalLandDiameter = 100f;
    private const float LongBridgeLength = 24.267f; // Bridge02, local Z length.
    private const float StoneBridgeLength = 24.847f; // Bridge04, local Z length.
    private const float RoadLength = 13.99f;

    private sealed class Island
    {
        public string Name;
        public Vector3 Center;
        public float Radius;
        public GameObject Object;
        public readonly List<float> OpenAngles = new List<float>();
    }

    [MenuItem("Tools/Goddess Trial/Generate Scene 1 - Large Forest Ruins V2")]
    public static void Generate()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureOutputFolder();

        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);

        GameObject mapRoot = NewRoot("SCENE_1_LARGE_FOREST_RUINS");
        GameObject environment = NewChild(mapRoot, "Environment");
        GameObject gameplay = NewChild(mapRoot, "Gameplay_Markers");
        GameObject lighting = NewChild(mapRoot, "Lighting");

        GameObject ocean = NewChild(environment, "00_Ocean");
        GameObject land = NewChild(environment, "01_Separated_LandMasses");
        GameObject bridges = NewChild(environment, "02_Bridges");
        GameObject roads = NewChild(environment, "03_Roads");
        GameObject rocks = NewChild(environment, "04_Rocks");
        GameObject trees = NewChild(environment, "05_Trees");
        GameObject plants = NewChild(environment, "06_Grass_And_Flowers");
        GameObject ruins = NewChild(environment, "07_Ruins_And_Landmarks");
        GameObject effects = NewChild(environment, "08_Fire_And_Crystals");
        GameObject background = NewChild(environment, "09_Background_Details");

        // The land top is at Y = 0 and the island cliffs extend downward.
        // A large ocean beneath the cliffs makes every land-mass silhouette clear.
        Place(
            "LandMass/Ocean.prefab",
            ocean.transform,
            "Deep_Ocean",
            new Vector3(0f, -48f, 220f),
            Quaternion.identity,
            new Vector3(0.75f, 1f, 0.75f));

        // Main route: about four times the usable land area of the first version.
        // Distances deliberately leave visible gaps between island edges.
        Island start = CreateIsland(
            land.transform, "01_Start_Island", "LandMass/LM100NAT01.prefab",
            new Vector3(0f, 0f, 0f), 0.58f, 8f);

        Island arena1 = CreateIsland(
            land.transform, "02_Forest_Arena", "LandMass/LM100NAT03.prefab",
            new Vector3(0f, 0f, 85f), 0.72f, -14f);

        Island grove = CreateIsland(
            land.transform, "03_Wild_Grove", "LandMass/LM100NAT06.prefab",
            new Vector3(-68f, 0f, 137f), 0.62f, 31f);

        Island crossroads = CreateIsland(
            land.transform, "04_Crossroads", "LandMass/LM100NAT08.prefab",
            new Vector3(4f, 0f, 194f), 0.68f, -21f);

        Island lake = CreateIsland(
            land.transform, "05_Lake_Sanctuary", "LandMass/LM100NAT04.prefab",
            new Vector3(72f, 0f, 247f), 0.60f, 17f);

        Island arena2 = CreateIsland(
            land.transform, "06_Ruined_Arena", "LandMass/LM100NAT09.prefab",
            new Vector3(3f, 0f, 303f), 0.78f, -32f);

        Island oldRuins = CreateIsland(
            land.transform, "07_Old_Ruins", "LandMass/LM100NAT02.prefab",
            new Vector3(-67f, 0f, 363f), 0.65f, 23f);

        Island boss = CreateIsland(
            land.transform, "08_Boss_Temple", "LandMass/LM100NAT07.prefab",
            new Vector3(0f, 0f, 448f), 0.85f, -8f);

        // Optional side islands increase exploration without lengthening the main route.
        Island rewardWest = CreateIsland(
            land.transform, "Side_Island_West_Reward", "LandMass/LM100NAT05.prefab",
            new Vector3(-76f, 0f, 218f), 0.50f, 42f);

        Island rewardEast = CreateIsland(
            land.transform, "Side_Island_East_Reward", "LandMass/LM100NAT01.prefab",
            new Vector3(82f, 0f, 330f), 0.48f, -27f);

        // Main bridges. Every bridge is scaled to the real gap between island edges.
        ConnectIslands(start, arena1, bridges.transform, roads.transform,
            "Bridge_Start_To_Arena1", "BuildingUtilityDeco/Bridge02.prefab",
            LongBridgeLength, 1.08f);

        ConnectIslands(arena1, grove, bridges.transform, roads.transform,
            "Bridge_Arena1_To_Grove", "BuildingUtilityDeco/Bridge04.prefab",
            StoneBridgeLength, 1.12f);

        ConnectIslands(grove, crossroads, bridges.transform, roads.transform,
            "Bridge_Grove_To_Crossroads", "BuildingUtilityDeco/Bridge02.prefab",
            LongBridgeLength, 1.10f);

        ConnectIslands(crossroads, lake, bridges.transform, roads.transform,
            "Bridge_Crossroads_To_Lake", "BuildingUtilityDeco/Bridge04.prefab",
            StoneBridgeLength, 1.12f);

        ConnectIslands(lake, arena2, bridges.transform, roads.transform,
            "Bridge_Lake_To_Arena2", "BuildingUtilityDeco/Bridge02.prefab",
            LongBridgeLength, 1.12f);

        ConnectIslands(arena2, oldRuins, bridges.transform, roads.transform,
            "Bridge_Arena2_To_OldRuins", "BuildingUtilityDeco/Bridge04.prefab",
            StoneBridgeLength, 1.16f);

        ConnectIslands(oldRuins, boss, bridges.transform, roads.transform,
            "Bridge_OldRuins_To_Boss", "BuildingUtilityDeco/Bridge02.prefab",
            LongBridgeLength, 1.18f);

        // Optional branches.
        ConnectIslands(crossroads, rewardWest, bridges.transform, roads.transform,
            "Bridge_To_West_Reward", "BuildingUtilityDeco/Bridge02.prefab",
            LongBridgeLength, 1.02f);

        ConnectIslands(arena2, rewardEast, bridges.transform, roads.transform,
            "Bridge_To_East_Reward", "BuildingUtilityDeco/Bridge02.prefab",
            LongBridgeLength, 1.02f);

        // Large landmarks make every zone readable from the isometric camera.
        BuildStartArea(start, ruins.transform, effects.transform);
        BuildArenaOne(arena1, rocks.transform, ruins.transform, effects.transform);
        BuildWildGrove(grove, rocks.transform, ruins.transform, effects.transform);
        BuildCrossroads(crossroads, ruins.transform, effects.transform);
        BuildLakeSanctuary(lake, ruins.transform, effects.transform);
        BuildArenaTwo(arena2, rocks.transform, ruins.transform, effects.transform);
        BuildOldRuins(oldRuins, rocks.transform, ruins.transform, effects.transform);
        BuildBossTemple(boss, rocks.transform, ruins.transform, effects.transform);
        BuildWestReward(rewardWest, ruins.transform, effects.transform);
        BuildEastReward(rewardEast, ruins.transform, effects.transform);

        // Dense edge decoration with clear openings around every bridge.
        DecorateIsland(start, trees.transform, rocks.transform, plants.transform, 1101, 17, 10, 26);
        DecorateIsland(arena1, trees.transform, rocks.transform, plants.transform, 1202, 25, 14, 36);
        DecorateIsland(grove, trees.transform, rocks.transform, plants.transform, 1303, 28, 13, 42);
        DecorateIsland(crossroads, trees.transform, rocks.transform, plants.transform, 1404, 22, 13, 35);
        DecorateIsland(lake, trees.transform, rocks.transform, plants.transform, 1505, 22, 12, 30);
        DecorateIsland(arena2, trees.transform, rocks.transform, plants.transform, 1606, 26, 17, 38);
        DecorateIsland(oldRuins, trees.transform, rocks.transform, plants.transform, 1707, 23, 15, 34);
        DecorateIsland(boss, trees.transform, rocks.transform, plants.transform, 1808, 28, 20, 38);
        DecorateIsland(rewardWest, trees.transform, rocks.transform, plants.transform, 1909, 18, 10, 24);
        DecorateIsland(rewardEast, trees.transform, rocks.transform, plants.transform, 2010, 17, 9, 24);

        // A few distant silhouettes frame the level without creating walkable routes.
        BuildBackground(background.transform);

        // Empty gameplay markers do not spoil the visual scene.
        BuildGameplayMarkers(gameplay.transform, start, arena1, grove, crossroads,
            lake, arena2, oldRuins, boss, rewardWest, rewardEast);

        BuildLightingAndAtmosphere(lighting.transform, mapRoot);

        EditorSceneManager.SaveScene(scene, OutputScene);
        Selection.activeGameObject = mapRoot;
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(OutputScene));

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            sceneView.pivot = new Vector3(0f, 0f, 230f);
            sceneView.rotation = Quaternion.Euler(48f, -32f, 0f);
            sceneView.size = 285f;
            sceneView.Repaint();
        }

        Debug.Log("Created " + OutputScene +
            ". Land masses are separated and connected by correctly scaled bridges.");

        EditorUtility.DisplayDialog(
            "Large Scene 1 created",
            "Created:\n" + OutputScene +
            "\n\nThe new map has separated land masses, visible ocean gaps, " +
            "properly scaled bridges, larger combat zones, two reward branches, " +
            "and hidden gameplay markers.\n\nBake NavMesh after adding your player and enemies.",
            "OK");
    }

    private static Island CreateIsland(
        Transform parent,
        string name,
        string prefabPath,
        Vector3 center,
        float horizontalScale,
        float yaw)
    {
        GameObject islandObject = Place(
            prefabPath,
            parent,
            name,
            center,
            Quaternion.Euler(0f, yaw, 0f),
            new Vector3(horizontalScale, 1f, horizontalScale));

        return new Island
        {
            Name = name,
            Center = center,
            Radius = NaturalLandDiameter * horizontalScale * 0.5f,
            Object = islandObject
        };
    }

    private static void ConnectIslands(
        Island a,
        Island b,
        Transform bridgeParent,
        Transform roadParent,
        string bridgeName,
        string bridgePrefab,
        float prefabLength,
        float bridgeWidthScale)
    {
        Vector3 direction = b.Center - a.Center;
        direction.y = 0f;
        float centerDistance = direction.magnitude;

        if (centerDistance < 0.01f)
            return;

        Vector3 forward = direction / centerDistance;
        float gap = centerDistance - a.Radius - b.Radius;

        if (gap <= 2f)
        {
            Debug.LogWarning(
                a.Name + " and " + b.Name +
                " are too close. Gap = " + gap.ToString("0.0") + " units.");
        }

        // Extend each bridge 2 units over both island edges so no jump is required.
        const float edgeOverlap = 2f;
        Vector3 bridgeStart = a.Center + forward * (a.Radius - edgeOverlap);
        Vector3 bridgeEnd = b.Center - forward * (b.Radius - edgeOverlap);
        float bridgeLength = Vector3.Distance(bridgeStart, bridgeEnd);
        Vector3 bridgeCenter = (bridgeStart + bridgeEnd) * 0.5f;

        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
        Place(
            bridgePrefab,
            bridgeParent,
            bridgeName,
            bridgeCenter + Vector3.up * 0.28f,
            rotation,
            new Vector3(
                bridgeWidthScale,
                1f,
                Mathf.Max(0.55f, bridgeLength / prefabLength)));

        a.OpenAngles.Add(DirectionAngle(a.Center, b.Center));
        b.OpenAngles.Add(DirectionAngle(b.Center, a.Center));

        // Stone/dirt paths connect each bridge entrance to its island center.
        Vector3 aRoadEnd = a.Center + forward * (a.Radius - 3f);
        Vector3 bRoadStart = b.Center - forward * (b.Radius - 3f);

        PlaceRoadLine(
            roadParent,
            bridgeName + "_Road_A",
            a.Center + forward * 3f,
            aRoadEnd,
            "RiverRoadLakeFall/RoadA01.prefab");

        PlaceRoadLine(
            roadParent,
            bridgeName + "_Road_B",
            bRoadStart,
            b.Center - forward * 3f,
            "RiverRoadLakeFall/RoadC01.prefab");
    }

    private static void PlaceRoadLine(
        Transform parent,
        string namePrefix,
        Vector3 start,
        Vector3 end,
        string roadPrefab)
    {
        Vector3 direction = end - start;
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance < 2f)
            return;

        Vector3 forward = direction / distance;
        int count = Mathf.Max(1, Mathf.CeilToInt(distance / 12.5f));
        float segmentLength = distance / count;
        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

        for (int i = 0; i < count; i++)
        {
            Vector3 position = start + forward * (segmentLength * i);
            Place(
                roadPrefab,
                parent,
                namePrefix + "_" + (i + 1).ToString("00"),
                position + Vector3.up * 0.08f,
                rotation,
                new Vector3(1.12f, 1f, segmentLength / RoadLength));
        }
    }

    private static void BuildStartArea(Island island, Transform ruins, Transform effects)
    {
        Place("BuildingUtilityDeco/Portal01.prefab", ruins, "Broken_Entrance_Portal",
            island.Center + new Vector3(0f, 0.25f, -11f),
            Quaternion.Euler(0f, 0f, 0f), Vector3.one * 1.2f);

        Place("BuildingUtilityDeco/Gate01.prefab", ruins, "Start_Stone_Gate",
            island.Center + new Vector3(0f, 0.18f, -6f),
            Quaternion.identity, new Vector3(1.25f, 1.2f, 1.25f));

        Place("BuildingUtilityDeco/SignPost01.prefab", ruins, "Start_Sign",
            island.Center + new Vector3(-7f, 0.15f, 5f),
            Quaternion.Euler(0f, 25f, 0f), Vector3.one);

        Place("BuildingUtilityDeco/Fire01.prefab", effects, "Start_Campfire",
            island.Center + new Vector3(8f, 0.15f, -2f),
            Quaternion.identity, Vector3.one);
    }

    private static void BuildArenaOne(Island island, Transform rocks, Transform ruins, Transform effects)
    {
        Place("Rock/RockCliff03.prefab", rocks, "Arena1_Central_Rock",
            island.Center + new Vector3(-2f, 0.15f, 1f),
            Quaternion.Euler(0f, 33f, 0f), new Vector3(1.7f, 1.25f, 1.6f));

        Place("BuildingUtilityDeco/WatchTower01.prefab", ruins, "Arena1_WatchTower",
            island.Center + new Vector3(20f, 0.1f, 11f),
            Quaternion.Euler(0f, -45f, 0f), new Vector3(1.1f, 1.1f, 1.1f));

        Place("BuildingUtilityDeco/WoodFence03.prefab", ruins, "Arena1_BrokenFence_A",
            island.Center + new Vector3(-17f, 0.1f, -9f),
            Quaternion.Euler(0f, 22f, 0f), Vector3.one * 1.25f);

        Place("BuildingUtilityDeco/WoodFence04.prefab", ruins, "Arena1_BrokenFence_B",
            island.Center + new Vector3(-13f, 0.1f, -14f),
            Quaternion.Euler(0f, 65f, 0f), Vector3.one * 1.15f);

        Place("BuildingUtilityDeco/Fire01.prefab", effects, "Arena1_Fire",
            island.Center + new Vector3(13f, 0.12f, -12f),
            Quaternion.identity, Vector3.one);
    }

    private static void BuildWildGrove(Island island, Transform rocks, Transform ruins, Transform effects)
    {
        Place("BuildingUtilityDeco/Well01.prefab", ruins, "Ancient_Grove_Well",
            island.Center + new Vector3(3f, 0.12f, 2f),
            Quaternion.Euler(0f, 20f, 0f), Vector3.one * 1.25f);

        Place("BuildingUtilityDeco/TreeStump01.prefab", ruins, "Grove_Old_Stump",
            island.Center + new Vector3(-11f, 0.08f, 8f),
            Quaternion.Euler(0f, 70f, 0f), Vector3.one * 1.4f);

        Place("Rock/RockCliff01.prefab", rocks, "Grove_Cliff_Rock",
            island.Center + new Vector3(13f, 0.12f, -8f),
            Quaternion.Euler(0f, 105f, 0f), Vector3.one * 1.3f);

        Place("BuildingUtilityDeco/Crystal01.prefab", effects, "Grove_Crystal",
            island.Center + new Vector3(-4f, 0.2f, -12f),
            Quaternion.identity, Vector3.one * 1.1f);
    }

    private static void BuildCrossroads(Island island, Transform ruins, Transform effects)
    {
        Place("BuildingUtilityDeco/SignPost06.prefab", ruins, "Crossroads_Sign",
            island.Center + new Vector3(-1f, 0.12f, 1f),
            Quaternion.Euler(0f, 15f, 0f), Vector3.one * 1.25f);

        Place("BuildingUtilityDeco/WoodBarrel01.prefab", ruins, "Crossroads_Barrel_A",
            island.Center + new Vector3(8f, 0.1f, -7f),
            Quaternion.Euler(0f, 28f, 0f), Vector3.one);

        Place("BuildingUtilityDeco/WoodBarrel01.prefab", ruins, "Crossroads_Barrel_B",
            island.Center + new Vector3(10f, 0.1f, -5f),
            Quaternion.Euler(0f, -15f, 0f), Vector3.one * 0.9f);

        Place("BuildingUtilityDeco/Fire01.prefab", effects, "Crossroads_Fire",
            island.Center + new Vector3(-9f, 0.12f, -8f),
            Quaternion.identity, Vector3.one);
    }

    private static void BuildLakeSanctuary(Island island, Transform ruins, Transform effects)
    {
        Place("RiverRoadLakeFall/Lake01.prefab", ruins, "Sanctuary_Lake",
            island.Center + new Vector3(0f, 0.18f, 1f),
            Quaternion.Euler(0f, 18f, 0f), new Vector3(0.78f, 1f, 0.78f));

        Place("BuildingUtilityDeco/Pillar01.prefab", ruins, "Lake_Pillar_A",
            island.Center + new Vector3(-16f, 0.15f, -9f),
            Quaternion.identity, Vector3.one * 1.15f);

        Place("BuildingUtilityDeco/Pillar02.prefab", ruins, "Lake_Pillar_B",
            island.Center + new Vector3(16f, 0.15f, -8f),
            Quaternion.identity, Vector3.one * 1.15f);

        Place("BuildingUtilityDeco/Crystal01.prefab", effects, "Lake_Crystal_A",
            island.Center + new Vector3(-8f, 0.22f, 10f),
            Quaternion.identity, Vector3.one * 0.9f);

        Place("BuildingUtilityDeco/Crystal01.prefab", effects, "Lake_Crystal_B",
            island.Center + new Vector3(9f, 0.22f, 11f),
            Quaternion.identity, Vector3.one * 0.8f);
    }

    private static void BuildArenaTwo(Island island, Transform rocks, Transform ruins, Transform effects)
    {
        Place("Rock/RockCliff05.prefab", rocks, "Arena2_Center_Cover",
            island.Center + new Vector3(0f, 0.16f, 0f),
            Quaternion.Euler(0f, 42f, 0f), new Vector3(1.9f, 1.35f, 1.8f));

        Place("Rock/Rock06.prefab", rocks, "Arena2_Left_Cover",
            island.Center + new Vector3(-18f, 0.14f, 8f),
            Quaternion.Euler(0f, 75f, 0f), Vector3.one * 1.55f);

        Place("Rock/Rock09.prefab", rocks, "Arena2_Right_Cover",
            island.Center + new Vector3(20f, 0.14f, -8f),
            Quaternion.Euler(0f, 15f, 0f), Vector3.one * 1.5f);

        Place("BuildingUtilityDeco/Pillar03.prefab", ruins, "Arena2_Ranged_Perch_A",
            island.Center + new Vector3(-24f, 0.12f, -15f),
            Quaternion.identity, Vector3.one * 1.25f);

        Place("BuildingUtilityDeco/Pillar04.prefab", ruins, "Arena2_Ranged_Perch_B",
            island.Center + new Vector3(23f, 0.12f, 15f),
            Quaternion.identity, Vector3.one * 1.25f);

        Place("BuildingUtilityDeco/Fire01.prefab", effects, "Arena2_Fire_A",
            island.Center + new Vector3(-12f, 0.12f, 17f),
            Quaternion.identity, Vector3.one);

        Place("BuildingUtilityDeco/Fire01.prefab", effects, "Arena2_Fire_B",
            island.Center + new Vector3(13f, 0.12f, -18f),
            Quaternion.identity, Vector3.one);
    }

    private static void BuildOldRuins(Island island, Transform rocks, Transform ruins, Transform effects)
    {
        Place("BuildingUtilityDeco/Gate03.prefab", ruins, "OldRuins_Gate",
            island.Center + new Vector3(1f, 0.16f, -8f),
            Quaternion.Euler(0f, -18f, 0f), Vector3.one * 1.35f);

        Place("BuildingUtilityDeco/Wall01.prefab", ruins, "OldRuins_Wall_A",
            island.Center + new Vector3(-15f, 0.12f, 3f),
            Quaternion.Euler(0f, 38f, 0f), new Vector3(1.5f, 1.2f, 1.5f));

        Place("BuildingUtilityDeco/Wall02.prefab", ruins, "OldRuins_Wall_B",
            island.Center + new Vector3(15f, 0.12f, 8f),
            Quaternion.Euler(0f, -32f, 0f), new Vector3(1.5f, 1.2f, 1.5f));

        Place("Rock/RockCliff02.prefab", rocks, "OldRuins_Rock",
            island.Center + new Vector3(-6f, 0.14f, 13f),
            Quaternion.Euler(0f, 80f, 0f), Vector3.one * 1.35f);

        Place("BuildingUtilityDeco/Crystal01.prefab", effects, "OldRuins_Crystal",
            island.Center + new Vector3(8f, 0.2f, -13f),
            Quaternion.identity, Vector3.one * 1.15f);
    }

    private static void BuildBossTemple(Island island, Transform rocks, Transform ruins, Transform effects)
    {
        Place("BuildingUtilityDeco/Gate04.prefab", ruins, "Boss_Temple_Gate",
            island.Center + new Vector3(0f, 0.2f, 25f),
            Quaternion.Euler(0f, 180f, 0f), Vector3.one * 2f);

        Place("BuildingUtilityDeco/Portal03.prefab", ruins, "Level_Exit_Portal",
            island.Center + new Vector3(0f, 0.25f, 31f),
            Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.3f);

        Place("BuildingUtilityDeco/Wall03.prefab", ruins, "Boss_Wall_Left",
            island.Center + new Vector3(-18f, 0.14f, 20f),
            Quaternion.Euler(0f, 18f, 0f), new Vector3(1.8f, 1.35f, 1.8f));

        Place("BuildingUtilityDeco/Wall04.prefab", ruins, "Boss_Wall_Right",
            island.Center + new Vector3(18f, 0.14f, 20f),
            Quaternion.Euler(0f, -18f, 0f), new Vector3(1.8f, 1.35f, 1.8f));

        Vector3[] pillarOffsets =
        {
            new Vector3(-25f, 0.14f, -13f),
            new Vector3(25f, 0.14f, -13f),
            new Vector3(-25f, 0.14f, 10f),
            new Vector3(25f, 0.14f, 10f)
        };

        for (int i = 0; i < pillarOffsets.Length; i++)
        {
            Place(
                "BuildingUtilityDeco/Pillar0" + ((i % 4) + 1) + ".prefab",
                ruins,
                "Boss_Pillar_" + (i + 1).ToString("00"),
                island.Center + pillarOffsets[i],
                Quaternion.identity,
                Vector3.one * 1.55f);
        }

        Place("Rock/RockCliff06.prefab", rocks, "Boss_Central_Altar_Rock",
            island.Center + new Vector3(0f, 0.14f, 1f),
            Quaternion.Euler(0f, 35f, 0f), new Vector3(1.45f, 1.15f, 1.45f));

        Place("BuildingUtilityDeco/Fire01.prefab", effects, "Boss_Fire_Left",
            island.Center + new Vector3(-12f, 0.14f, 18f),
            Quaternion.identity, Vector3.one * 1.25f);

        Place("BuildingUtilityDeco/Fire01.prefab", effects, "Boss_Fire_Right",
            island.Center + new Vector3(12f, 0.14f, 18f),
            Quaternion.identity, Vector3.one * 1.25f);

        // Mountain silhouettes make the final arena visually dominant.
        Place("Mountains/RockMountain03.prefab", ruins, "Boss_Back_Mountain_Left",
            island.Center + new Vector3(-36f, 0f, 31f),
            Quaternion.Euler(0f, 15f, 0f), Vector3.one * 1.35f);

        Place("Mountains/RockMountain05.prefab", ruins, "Boss_Back_Mountain_Right",
            island.Center + new Vector3(35f, 0f, 32f),
            Quaternion.Euler(0f, -25f, 0f), Vector3.one * 1.3f);
    }

    private static void BuildWestReward(Island island, Transform ruins, Transform effects)
    {
        Place("BuildingUtilityDeco/Crystal01.prefab", effects, "West_Reward_Crystal_Main",
            island.Center + new Vector3(0f, 0.2f, 2f),
            Quaternion.identity, Vector3.one * 1.6f);

        Place("BuildingUtilityDeco/WoodBarrel01.prefab", ruins, "West_Reward_Barrel_A",
            island.Center + new Vector3(-6f, 0.1f, -4f),
            Quaternion.Euler(0f, 25f, 0f), Vector3.one);

        Place("BuildingUtilityDeco/WoodBarrel01.prefab", ruins, "West_Reward_Barrel_B",
            island.Center + new Vector3(6f, 0.1f, -4f),
            Quaternion.Euler(0f, -20f, 0f), Vector3.one);
    }

    private static void BuildEastReward(Island island, Transform ruins, Transform effects)
    {
        Place("BuildingUtilityDeco/Portal02.prefab", ruins, "East_Reward_Shrine",
            island.Center + new Vector3(0f, 0.2f, 5f),
            Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.1f);

        Place("BuildingUtilityDeco/Crystal01.prefab", effects, "East_Reward_Crystal_A",
            island.Center + new Vector3(-6f, 0.2f, -3f),
            Quaternion.identity, Vector3.one * 0.9f);

        Place("BuildingUtilityDeco/Crystal01.prefab", effects, "East_Reward_Crystal_B",
            island.Center + new Vector3(6f, 0.2f, -3f),
            Quaternion.identity, Vector3.one * 0.9f);
    }

    private static void DecorateIsland(
        Island island,
        Transform treeParent,
        Transform rockParent,
        Transform plantParent,
        int seed,
        int treeCount,
        int rockCount,
        int plantCount)
    {
        System.Random random = new System.Random(seed);

        for (int i = 0; i < treeCount; i++)
        {
            float angle = FindAllowedAngle(island, random, 18f);
            float radius = Mathf.Lerp(
                island.Radius * 0.66f,
                island.Radius * 0.88f,
                (float)random.NextDouble());

            Vector3 position = PolarPosition(island.Center, angle, radius, 0.08f);
            int variant = random.Next(1, 6);
            float scale = Mathf.Lerp(0.9f, 1.45f, (float)random.NextDouble());

            Place(
                "TreePlants/Tree0" + variant + ".prefab",
                treeParent,
                island.Name + "_Tree_" + (i + 1).ToString("00"),
                position,
                Quaternion.Euler(0f, random.Next(0, 360), 0f),
                Vector3.one * scale);
        }

        for (int i = 0; i < rockCount; i++)
        {
            float angle = FindAllowedAngle(island, random, 15f);
            float radius = Mathf.Lerp(
                island.Radius * 0.73f,
                island.Radius * 0.94f,
                (float)random.NextDouble());

            Vector3 position = PolarPosition(island.Center, angle, radius, 0.09f);
            int variant = random.Next(1, 11);
            float scale = Mathf.Lerp(0.75f, 1.45f, (float)random.NextDouble());

            Place(
                "Rock/Rock" + variant.ToString("00") + ".prefab",
                rockParent,
                island.Name + "_Rock_" + (i + 1).ToString("00"),
                position,
                Quaternion.Euler(0f, random.Next(0, 360), 0f),
                Vector3.one * scale);
        }

        for (int i = 0; i < plantCount; i++)
        {
            float angle = (float)random.NextDouble() * 360f;
            float radius = Mathf.Lerp(
                island.Radius * 0.22f,
                island.Radius * 0.78f,
                Mathf.Sqrt((float)random.NextDouble()));

            Vector3 position = PolarPosition(island.Center, angle, radius, 0.045f);
            bool flower = random.NextDouble() > 0.72;
            string prefab = flower
                ? "TreePlants/Flower0" + random.Next(1, 6) + ".prefab"
                : "TreePlants/Grass0" + random.Next(1, 8) + ".prefab";

            float scale = Mathf.Lerp(0.8f, 1.25f, (float)random.NextDouble());
            Place(
                prefab,
                plantParent,
                island.Name + "_Plant_" + (i + 1).ToString("00"),
                position,
                Quaternion.Euler(0f, random.Next(0, 360), 0f),
                Vector3.one * scale);
        }
    }

    private static float FindAllowedAngle(
        Island island,
        System.Random random,
        float clearance)
    {
        for (int attempt = 0; attempt < 40; attempt++)
        {
            float angle = (float)random.NextDouble() * 360f;
            bool blocked = false;

            for (int i = 0; i < island.OpenAngles.Count; i++)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(angle, island.OpenAngles[i])) < clearance)
                {
                    blocked = true;
                    break;
                }
            }

            if (!blocked)
                return angle;
        }

        return (float)random.NextDouble() * 360f;
    }

    private static void BuildBackground(Transform parent)
    {
        Vector3[] positions =
        {
            new Vector3(-180f, -15f, 40f),
            new Vector3(170f, -18f, 100f),
            new Vector3(-185f, -20f, 270f),
            new Vector3(180f, -18f, 390f),
            new Vector3(-165f, -15f, 500f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            Place(
                "LandMass/LM20RND.prefab",
                parent,
                "Distant_Island_" + (i + 1).ToString("00"),
                positions[i],
                Quaternion.Euler(0f, i * 51f, 0f),
                new Vector3(1.8f, 1f, 1.8f));

            Place(
                "Mountains/Mountain0" + ((i % 7) + 1) + ".prefab",
                parent,
                "Distant_Mountain_" + (i + 1).ToString("00"),
                positions[i] + new Vector3(0f, 0.2f, 0f),
                Quaternion.Euler(0f, i * 67f, 0f),
                Vector3.one * 1.8f);
        }
    }

    private static void BuildGameplayMarkers(
        Transform parent,
        Island start,
        Island arena1,
        Island grove,
        Island crossroads,
        Island lake,
        Island arena2,
        Island oldRuins,
        Island boss,
        Island rewardWest,
        Island rewardEast)
    {
        EmptyMarker(parent, "PlayerSpawn", start.Center + new Vector3(0f, 0.5f, -2f));

        CreateArenaMarkers(parent, "Arena01", arena1.Center, 13f, 5, 18f);
        CreateArenaMarkers(parent, "GroveEncounter", grove.Center, 11f, 4, 45f);
        CreateArenaMarkers(parent, "CrossroadsEncounter", crossroads.Center, 12f, 5, 5f);
        CreateArenaMarkers(parent, "LakeEncounter", lake.Center, 11f, 4, 30f);
        CreateArenaMarkers(parent, "Arena02", arena2.Center, 17f, 7, 12f);
        CreateArenaMarkers(parent, "OldRuinsEncounter", oldRuins.Center, 13f, 5, 50f);
        CreateArenaMarkers(parent, "BossArena", boss.Center, 19f, 6, 0f);
        CreateArenaMarkers(parent, "WestRewardGuard", rewardWest.Center, 9f, 3, 20f);
        CreateArenaMarkers(parent, "EastRewardGuard", rewardEast.Center, 8f, 3, 65f);

        EmptyMarker(parent, "LevelExit", boss.Center + new Vector3(0f, 0.5f, 31f));
    }

    private static void CreateArenaMarkers(
        Transform parent,
        string groupName,
        Vector3 center,
        float radius,
        int count,
        float startAngle)
    {
        GameObject group = NewChild(parent.gameObject, groupName);
        EmptyMarker(group.transform, groupName + "_Center", center + Vector3.up * 0.4f);

        for (int i = 0; i < count; i++)
        {
            float angle = (startAngle + i * (360f / count)) * Mathf.Deg2Rad;
            Vector3 position = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0.4f,
                Mathf.Sin(angle) * radius);

            EmptyMarker(
                group.transform,
                "EnemySpawn_" + (i + 1).ToString("00"),
                position);
        }
    }

    private static void EmptyMarker(Transform parent, string name, Vector3 position)
    {
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one;
    }

    private static void BuildLightingAndAtmosphere(Transform parent, GameObject mapRoot)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.42f, 0.58f, 0.62f, 1f);
        RenderSettings.fogDensity = 0.0017f;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.54f, 0.67f, 0.72f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.34f, 0.43f, 0.39f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.16f, 0.20f, 0.18f, 1f);

        GameObject sun = new GameObject("Directional Light");
        sun.transform.SetParent(parent);
        sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

        Light sunLight = sun.AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.color = new Color(1f, 0.91f, 0.76f, 1f);
        sunLight.intensity = 1.18f;
        sunLight.shadows = LightShadows.Soft;
        sunLight.shadowStrength = 0.82f;

        GameObject fill = new GameObject("Soft Fill Light");
        fill.transform.SetParent(parent);
        fill.transform.position = new Vector3(-80f, 70f, 250f);

        Light fillLight = fill.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.transform.rotation = Quaternion.Euler(55f, 145f, 0f);
        fillLight.color = new Color(0.43f, 0.56f, 0.72f, 1f);
        fillLight.intensity = 0.28f;
        fillLight.shadows = LightShadows.None;

        GameObject previewCamera = new GameObject("EDITOR_PREVIEW_CAMERA_DISABLED");
        previewCamera.transform.SetParent(parent);
        previewCamera.transform.position = new Vector3(170f, 190f, -115f);
        previewCamera.transform.rotation = Quaternion.Euler(43f, -31f, 0f);

        Camera camera = previewCamera.AddComponent<Camera>();
        camera.fieldOfView = 48f;
        camera.farClipPlane = 1200f;
        camera.enabled = false;
        previewCamera.tag = "Untagged";

        // Keep the generated hierarchy easy to select as one object.
        mapRoot.transform.position = Vector3.zero;
    }

    private static float DirectionAngle(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        return Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
    }

    private static Vector3 PolarPosition(
        Vector3 center,
        float angleDegrees,
        float radius,
        float y)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        return center + new Vector3(
            Mathf.Cos(radians) * radius,
            y,
            Mathf.Sin(radians) * radius);
    }

    private static GameObject Place(
        string relativePath,
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale)
    {
        string fullPath = BasePath + relativePath;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        GameObject instance;

        if (prefab != null)
        {
            instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        }
        else
        {
            instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.name = "MISSING_" + name;
            instance.transform.localScale = new Vector3(2f, 0.5f, 2f);
            Debug.LogWarning("Missing prefab: " + fullPath);
        }

        instance.name = name;
        instance.transform.SetParent(parent, true);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = scale;
        return instance;
    }

    private static GameObject NewRoot(string name)
    {
        return new GameObject(name);
    }

    private static GameObject NewChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform);
        return child;
    }

    private static void EnsureOutputFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
    }
}
#endif
