import os
import re

files_to_tag = {
    "Villager.cs": {"Origin": "Stacklands", "Reason": "Legacy villager economy gameplay", "RemovalPhase": 2},
    "BaseVillager.cs": {"Origin": "Stacklands", "Reason": "Legacy villager economy gameplay", "RemovalPhase": 2},
    "OldVillager.cs": {"Origin": "Stacklands", "Reason": "Legacy villager economy gameplay", "RemovalPhase": 2},
    "TeenageVillager.cs": {"Origin": "Stacklands", "Reason": "Legacy villager economy gameplay", "RemovalPhase": 2},
    "Worker.cs": {"Origin": "Stacklands", "Reason": "Legacy worker mechanics", "RemovalPhase": 2},
    "Happiness.cs": {"Origin": "Cities", "Reason": "Legacy happiness economy", "RemovalPhase": 1},
    "Unhappiness.cs": {"Origin": "Cities", "Reason": "Legacy happiness economy", "RemovalPhase": 1},
    "Pollution.cs": {"Origin": "Cities", "Reason": "Legacy pollution mechanics", "RemovalPhase": 1},
    "WellbeingGenerator.cs": {"Origin": "Cities", "Reason": "Legacy wellbeing mechanics", "RemovalPhase": 1},
    "Dollar.cs": {"Origin": "Cities", "Reason": "Legacy currency", "RemovalPhase": 2},
    "Creditcard.cs": {"Origin": "Cities", "Reason": "Legacy currency", "RemovalPhase": 2},
    "EnergyConsumer.cs": {"Origin": "Stacklands", "Reason": "Legacy industrial energy", "RemovalPhase": 2},
    "EnergyGenerator.cs": {"Origin": "Stacklands", "Reason": "Legacy industrial energy", "RemovalPhase": 2},
    "EnergyHarvestable.cs": {"Origin": "Stacklands", "Reason": "Legacy industrial energy", "RemovalPhase": 2},
    "ConsumingEnergyGenerator.cs": {"Origin": "Stacklands", "Reason": "Legacy industrial energy", "RemovalPhase": 2},
    "PassiveEnergyConsumer.cs": {"Origin": "Stacklands", "Reason": "Legacy industrial energy", "RemovalPhase": 2},
    "PassiveEnergyGenerator.cs": {"Origin": "Stacklands", "Reason": "Legacy industrial energy", "RemovalPhase": 2},
    "TransmissionTower.cs": {"Origin": "Stacklands", "Reason": "Legacy industrial energy", "RemovalPhase": 2},
    "SewerCard.cs": {"Origin": "Cities", "Reason": "Legacy sewer mechanics", "RemovalPhase": 1},
    "SepticTank.cs": {"Origin": "Cities", "Reason": "Legacy sewer mechanics", "RemovalPhase": 1},
    "WaterTreatmentPlant.cs": {"Origin": "Cities", "Reason": "Legacy sewer mechanics", "RemovalPhase": 1},
    "IndustrialRevolution.cs": {"Origin": "Stacklands", "Reason": "Legacy industrial mechanics", "RemovalPhase": 1},
    "IndustrialSmelter.cs": {"Origin": "Stacklands", "Reason": "Legacy industrial mechanics", "RemovalPhase": 1},
    "Factory.cs": {"Origin": "Stacklands", "Reason": "Legacy factory mechanics", "RemovalPhase": 1},
    "FactoryParts.cs": {"Origin": "Stacklands", "Reason": "Legacy factory mechanics", "RemovalPhase": 1},
    "ToyFactory.cs": {"Origin": "Stacklands", "Reason": "Legacy factory mechanics", "RemovalPhase": 1},
    "Smelter.cs": {"Origin": "Stacklands", "Reason": "Legacy industrial mechanics", "RemovalPhase": 1},
    "Royal.cs": {"Origin": "Cities", "Reason": "Legacy cities royal mechanic", "RemovalPhase": 1},
    "RoyalBuilding.cs": {"Origin": "Cities", "Reason": "Legacy cities royal mechanic", "RemovalPhase": 1},
    "AngryRoyal.cs": {"Origin": "Cities", "Reason": "Legacy cities royal mechanic", "RemovalPhase": 1},
    "CityHall.cs": {"Origin": "Cities", "Reason": "Legacy cities mechanic", "RemovalPhase": 1},
    "Apartment.cs": {"Origin": "Cities", "Reason": "Legacy cities housing", "RemovalPhase": 1},
    "House.cs": {"Origin": "Cities", "Reason": "Legacy cities housing", "RemovalPhase": 1},
    "Landmark.cs": {"Origin": "Cities", "Reason": "Legacy cities landmark", "RemovalPhase": 1},
    "FoodWarehouse.cs": {"Origin": "Stacklands", "Reason": "Legacy survival mechanic", "RemovalPhase": 2},
    "Harvestable.cs": {"Origin": "Stacklands", "Reason": "Legacy survival mechanic", "RemovalPhase": 2},
    "Farmland.cs": {"Origin": "Stacklands", "Reason": "Legacy survival mechanic", "RemovalPhase": 2},
    "Garden.cs": {"Origin": "Stacklands", "Reason": "Legacy survival mechanic", "RemovalPhase": 2},
}

base_dir = r"c:\Users\Administrator\Documents\GitHub\full-code-demo\GameScripts\Cards\Data"

for file, data in files_to_tag.items():
    filepath = os.path.join(base_dir, file)
    if os.path.exists(filepath):
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
        
        if "[LegacyContent" not in content:
            attr = f'[LegacyContent(Origin = "{data["Origin"]}", Reason = "{data["Reason"]}", RemovalPhase = {data["RemovalPhase"]})]\n'
            # Find class declaration
            pattern = r'(public\s+(?:abstract\s+|sealed\s+|partial\s+)?class\s+\w+)'
            content = re.sub(pattern, attr + r'\1', content, count=1)
            
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"Tagged {file}")
    else:
        print(f"File not found: {file}")
