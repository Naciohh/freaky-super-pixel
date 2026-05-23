ENEMY DATA ASSETS - Create These in Unity Editor
================================================

This file documents the EnemyData ScriptableObject instances that need to be created manually in the Unity Editor.

To create an asset:
1. Right-click in Project window (Assets/Data folder)
2. Select "Create > Game > Enemy Data"
3. Set the values below
4. Save with the specified name


ASSET 1: Enemy_Small
====================
File Name: Enemy_Small.asset
displayName: "Esqueleto"
isBoss: false
maxHP: 30
damage: 10
moveSpeed: 3.5
modelScale: 0.7
attackCooldown: 1.2
bossAttackType: Melee


ASSET 2: Enemy_Medium
====================
File Name: Enemy_Medium.asset
displayName: "Esqueleto"
isBoss: false
maxHP: 60
damage: 25
moveSpeed: 2.5
modelScale: 1.0
attackCooldown: 1.2
bossAttackType: Melee


ASSET 3: Enemy_Large
====================
File Name: Enemy_Large.asset
displayName: "Esqueleto"
isBoss: false
maxHP: 100
damage: 40
moveSpeed: 1.8
modelScale: 1.4
attackCooldown: 1.5
bossAttackType: Melee


ASSET 4: Boss_Supermarket
====================
File Name: Boss_Supermarket.asset
displayName: "Jefe del Súper"
isBoss: true
maxHP: 500
damage: 60
moveSpeed: 2.0
modelScale: 2.0
attackCooldown: 0.8
bossAttackType: Melee
