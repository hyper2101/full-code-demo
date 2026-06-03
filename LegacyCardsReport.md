# [LEGACY AUDIT]

## Cards.cs Audit
- **Legacy Registrations Detected:**
  - `villager`, `base_villager`, `old_villager`, `teenage_villager`, `worker`
  - `happiness`, `unhappiness`, `pollution`, `wellbeing_generator`
  - `dollar`, `creditcard`
  - `energy_consumer`, `energy_generator`, `energy_harvestable`, `consuming_energy_generator`, `passive_energy_consumer`, `passive_energy_generator`, `transmission_tower`
  - `sewer`, `septic_tank`, `water_treatment_plant`
  - `industrial_revolution`, `industrial_smelter`, `factory`, `factory_parts`, `toy_factory`, `smelter`
  - `royal`, `royal_building`, `angry_royal`, `city_hall`, `apartment`, `house`, `landmark`
  - `food_warehouse`, `harvestable`, `farmland`, `garden`

## Splitting Plan
The constants related to the above systems will be moved to `LegacyCards.cs`.
Constants related to core gameplay and combat (like `cat_basic`, `cat_corpse`, etc.) will be moved to `DogmaCards.cs` or kept in `Cards.cs` as partials.
Constants for prototypes/experiments will go to `ExperimentalCards.cs`.
