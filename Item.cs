using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BattleNet
{
    class ItemEntry
    {

        public String Name;
        public String Type;
        public Item.ClassificationType Classification;
        public UInt32 Width, Height;
        public Boolean Stackable, Usable, Throwable;

        public ItemEntry()
        {

        }
        public ItemEntry(String name, String type, Item.ClassificationType classification, UInt32 width, UInt32 height, Boolean stackable, Boolean usable, Boolean throwable)
        {
            Name            = name;
            Type            = type;
            Classification  = classification;
            Width           = width;
            Height          = height;
            Stackable       = stackable;
            Usable          = usable;
            Throwable       = throwable;
        }

        public Boolean IsArmor()
        {
            switch (Classification)
            {
                case Item.ClassificationType.helm:
                case Item.ClassificationType.armor:
                case Item.ClassificationType.shield:
                case Item.ClassificationType.gloves:
                case Item.ClassificationType.boots:
                case Item.ClassificationType.belt:
                case Item.ClassificationType.druid_pelt:
                case Item.ClassificationType.barbarian_helm:
                case Item.ClassificationType.paladin_shield:
                case Item.ClassificationType.necromancer_shrunken_head:
                case Item.ClassificationType.circlet:
                    return true;
            }
            return false;
        }
        public Boolean IsWeapon()
        {
            switch (Classification)
            {           
                case Item.ClassificationType.amazon_bow:
                case Item.ClassificationType.amazon_javelin:
                case Item.ClassificationType.amazon_spear:
                case Item.ClassificationType.assassin_katar:
                case Item.ClassificationType.axe:
                case Item.ClassificationType.bow:
                case Item.ClassificationType.club:
                case Item.ClassificationType.crossbow:
                case Item.ClassificationType.dagger:
                case Item.ClassificationType.hammer:
                case Item.ClassificationType.javelin:
                case Item.ClassificationType.mace:
                case Item.ClassificationType.polearm:
                case Item.ClassificationType.scepter:
                case Item.ClassificationType.sorceress_orb:
                case Item.ClassificationType.spear:
                case Item.ClassificationType.sword:
                case Item.ClassificationType.staff:
                case Item.ClassificationType.throwing_axe:
                case Item.ClassificationType.throwing_knife:
                case Item.ClassificationType.throwing_potion:
                case Item.ClassificationType.wand:
                    return true;
            }
            return false;
        }
    }
  
    public class Item
    {
        public Item()
        {
            prefixes = new List<uint>();
            suffixes = new List<uint>();
            properties = new List<PropertyType>();
            sockets = UInt32.MaxValue;
        }
        public byte[] packet;

        public UInt32 action;

        [XmlElement(ElementName= "Name")]
        public String name;
        [XmlElement(typeof(bool),ElementName= "IsEthereal")]
        public bool ethereal;
        [XmlElement(typeof(bool), ElementName = "HasSockets")]
        public bool has_sockets;
        [XmlElement(ElementName = "SocketCount")]
        public UInt32 sockets;
        [XmlElement(typeof(QualityType),ElementName = "ItemQuality")]
        public QualityType quality;
        [XmlElement(ElementName = "Type")]
        public String type;



        public Int32 width;
        public Int32 height;

        public UInt32 category;
        public UInt32 id;
        public bool equipped;
        public bool in_socket;
        public bool identified;
        public bool switched_in;
        public bool switched_out;
        public bool broken;
        public bool potion;
        
        public bool in_store;
        public bool not_in_a_socket;
        public bool ear;
        public bool start_item;
        public bool simple_item;
       
        public bool personalised;
        public bool gambling;
        public bool rune_word;

        public bool ground;

        public VersionType version;

        public bool unspecified_directory;
        public UInt32 x;
        public UInt32 y;
        public UInt32 directory;
        public ContainerType container;

        //public GameData.CharacterClassType ear_character_class;
        public UInt32 ear_level;
        public String ear_name;
        

        public bool is_gold;
        public UInt32 amount;

        public UInt32 used_sockets;
        public UInt32 level;

        public bool has_graphic;
        public UInt32 graphic;

        public bool has_colour;
        public UInt32 colour;

        public UInt32 prefix;
        public UInt32 suffix;

        public List<UInt32> prefixes;
        public List<UInt32> suffixes;

        public SuperiorItemClassType superiority;

        public UInt32 set_code;
        public UInt32 unique_code;

        public UInt32 runeword_id;
        public UInt32 runeword_parameter;

        public String personalised_name;

        public bool is_armor;
        public bool is_weapon;
        public UInt32 defense;

        public UInt32 indestructible;
        public UInt32 durability;
        public UInt32 maximum_durability;

        

        public List<PropertyType> properties;


	    public static bool operator<(Item first, Item other)
        {
            return false;
        }

        public static bool operator >(Item first, Item other)
        {
            return false;
        }

        public class PropertyType
        {
            item_stat_type stat;
            Int32 value;

            UInt32 minimum;
            UInt32 maximum;
            UInt32 length;

            UInt32 level;
            UInt32 character_class;
            UInt32 skill;
            UInt32 tab;

            UInt32 monster;

            UInt32 charges;
            UInt32 maximum_charges;

            UInt32 skill_chance;

            UInt32 per_level;

            PropertyType()
            {

            }
        };

        public enum VersionType
        {
            classic = 0,
            classic110 = 2,
            lod = 100,
            lod110 = 101
        };

        public enum ClassType
        {
            not_applicable,
            normal,
            exceptional,
            elite
        };

        public enum QualityType
        {
            not_applicable,
            inferior,
            normal,
            superior,
            magical,
            set,
            rare,
            unique,
            crafted
        };

        public enum DirectoryType
        {
            not_applicable = 0,
            helm = 1,
            amulet = 2,
            armor = 3,
            right_hand = 4,
            left_hand = 5,
            right_hand_ring = 6,
            left_hand_ring = 7,
            belt = 8,
            boots = 9,
            gloves = 10,
            right_hand_switch = 11,
            left_hand_switch = 12
        };

        public enum ContainerType
        {
            unspecified = 0,
            inventory = 2,
            trader_offer = 4,
            for_trade = 6,
            cube = 8,
            stash = 0x0A,
            belt = 0x20,
            item = 0x40,
            armor_tab = 0x82,
            weapon_tab_1 = 0x84,
            weapon_tab_2 = 0x86,
            misc_tab = 0x88,
        };

        public enum Action
        {
            add_to_ground = 0,
            ground_to_cursor = 1,
            drop_to_ground = 2,
            on_ground = 3,
            put_in_container = 4,
            remove_from_container = 5,
            equip = 6,
            indirectly_swap_body_item = 7,
            unequip = 8,
            swap_body_item = 9,
            add_quantity = 0x0A,
            add_to_shop = 0x0B,
            remove_from_shop = 0x0C,
            swap_in_container = 0x0D,
            put_in_belt = 0x0E,
            remove_from_belt = 0x0F,
            swap_in_belt = 0x10,
            auto_unequip = 0x11,
            to_cursor = 0x12,
            item_in_socket = 0x13,
            update_stats = 0x15,
            weapon_switch = 0x17
        };

        public enum item_container_gc_type
        {
            inventory = 0,
            trade = 2,
            cube = 3,
            stash = 4,
        };

        public enum item_destination_type
        {
            unspecified = 0,
            equipment = 1,
            belt = 2,
            ground = 3,
            cursor = 4,
            item = 6,
        };

        public enum SuperiorItemClassType
        {
            not_applicable = -1,
            attackRating = 0,
            damage = 1,
            defense = 2,
            damage_attack_rating = 3,
            durability = 4,
            attack_rating_durability = 5,
            damage_durability = 6,
            defense_durability = 7
        };

        public enum ClassificationType
        {
            amazon_bow,
            amazon_javelin,
            amazon_spear,
            amulet,
            antidote_potion,
            armor,
            arrows,
            assassin_katar,
            axe,
            barbarian_helm,
            belt,
            body_part,
            bolts,
            boots,
            bow,
            circlet,
            club,
            crossbow,
            dagger,
            druid_pelt,
            ear,
            elixir,
            gem,
            gloves,
            gold,
            grand_charm,
            hammer,
            health_potion,
            helm,
            herb,
            javelin,
            jewel,
            key,
            large_charm,
            mace,
            mana_potion,
            necromancer_shrunken_head,
            paladin_shield,
            polearm,
            quest_item,
            rejuvenation_potion,
            ring,
            rune,
            scepter,
            scroll,
            shield,
            small_charm,
            sorceress_orb,
            spear,
            staff,
            stamina_potion,
            sword,
            thawing_potion,
            throwing_axe,
            throwing_knife,
            throwing_potion,
            tome,
            torch,
            wand
        };

        public enum item_stat_type
        {
            strength,
            energy,
            dexterity,
            vitality,
            all_attributes,
            new_skills,
            life,
            maximum_life,
            mana,
            maximum_mana,
            stamina,
            maximum_stamina,
            level,
            experience,
            gold,
            bank,
            enhanced_defense,
            enhanced_maximum_damage,
            enhanced_minimum_damage,
            attack_rating,
            increased_blocking,
            minimum_damage,
            maximum_damage,
            secondary_minimum_damage,
            secondary_maximum_damage,
            enhanced_damage,
            mana_recovery,
            mana_recovery_bonus,
            stamina_recovery_bonus,
            last_experience,
            next_experience,
            defense,
            defense_vs_missiles,
            defense_vs_melee,
            damage_reduction,
            magical_damage_reduction,
            damage_reduction_percent,
            magical_damage_reduction_percent,
            maximum_magical_damage_reduction_percent,
            fire_resistance,
            maximum_fire_resistance,
            lightning_resistance,
            maximum_lightning_resistance,
            cold_resistance,
            maximum_cold_resistance,
            poison_resistance,
            maximum_poison_resistance,
            damage_aura,
            minimum_fire_damage,
            maximum_fire_damage,
            minimum_lightning_damage,
            maximum_lightning_damage,
            minimum_magical_damage,
            maximum_magical_damage,
            minimum_cold_damage,
            maximum_cold_damage,
            cold_damage_length,
            minimum_poison_damage,
            maximum_poison_damage,
            poison_damage_length,
            minimum_life_stolen_per_hit,
            maximum_life_stolen_per_hit,
            minimum_mana_stolen_per_hit,
            maximum_mana_stolen_per_hit,
            minimum_stamina_drain,
            maximum_stamina_drain,
            stun_length,
            velocity_percent,
            attack_rate,
            other_animation_rate,
            quantity,
            value,
            durability,
            maximum_durability,
            replenish_life,
            enhanced_maximum_durability,
            enhanced_life,
            enhanced_mana,
            attacker_takes_damage,
            extra_gold,
            better_chance_of_getting_magic_item,
            knockback,
            time_duration,
            class_skills,
            unsent_parameter,
            add_experience,
            life_after_each_kill,
            reduce_vendor_prices,
            double_herb_duration,
            light_radius,
            light_colour,
            reduced_requirements,
            reduced_level_requirement,
            increased_attack_speed,
            reduced_level_requirement_percent,
            last_block_frame,
            faster_run_walk,
            non_class_skill,
            state,
            faster_hit_recovery,
            monster_player_count,
            skill_poison_override_length,
            faster_block_rate,
            skill_bypass_undead,
            skill_bypass_demons,
            faster_cast_rate,
            skill_bypass_beasts,
            single_skill,
            slain_monsters_rest_in_peace,
            curse_resistance,
            poison_length_reduction,
            adds_damage,
            hit_causes_monster_to_flee,
            hit_blinds_target,
            damage_to_mana,
            ignore_targets_defense,
            reduce_targets_defense,
            prevent_monster_heal,
            half_freeze_duration,
            to_hit_percent,
            monster_defense_reduction_per_hit,
            damage_to_demons,
            damage_to_undead,
            attack_rating_against_demons,
            attack_rating_against_undead,
            throwable,
            elemental_skills,
            all_skills,
            attacker_takes_lightning_damage,
            iron_maiden_level,
            lifetap_level,
            thorns_percent,
            bone_armor,
            maximum_bone_armor,
            freezes_target,
            open_wounds,
            crushing_blow,
            kick_damage,
            mana_after_each_kill,
            life_after_each_demon_kill,
            extra_blood,
            deadly_strike,
            fire_absorption_percent,
            fire_absorption,
            lightning_absorption_percent,
            lightning_absorption,
            magic_absorption_percent,
            magic_absorption,
            cold_absorption_percent,
            cold_absorption,
            slows_down_enemies,
            aura,
            indestructible,
            cannot_be_frozen,
            stamina_drain_percent,
            reanimate,
            piercing_attack,
            fires_magic_arrows,
            fire_explosive_arrows,
            minimum_throwing_damage,
            maximum_throwing_damage,
            skill_hand_of_athena,
            skill_stamina_percent,
            skill_passive_stamina_percent,
            concentration,
            enchant,
            pierce,
            conviction,
            chilling_armor,
            frenzy,
            decrepify,
            skill_armor_percent,
            alignment,
            target_0,
            target_1,
            gold_lost,
            conversion_level,
            conversion_maximum_life,
            unit_do_overlay,
            attack_rating_against_monster_type,
            damage_to_monster_type,
            fade,
            armor_override_percent,
            unused_183,
            unused_184,
            unused_185,
            unused_186,
            unused_187,
            skill_tab,
            unused_189,
            unused_190,
            unused_191,
            unused_192,
            unused_193,
            socket_count,
            skill_on_striking,
            skill_on_kill,
            skill_on_death,
            skill_on_hit,
            skill_on_level_up,
            unused_200,
            skill_when_struck,
            unused_202,
            unused_203,
            charged,
            unused_204,
            unused_205,
            unused_206,
            unused_207,
            unused_208,
            unused_209,
            unused_210,
            unused_211,
            unused_212,
            defense_per_level,
            enhanced_defense_per_level,
            life_per_level,
            mana_per_level,
            maximum_damage_per_level,
            maximum_enhanced_damage_per_level,
            strength_per_level,
            dexterity_per_level,
            energy_per_level,
            vitality_per_level,
            attack_rating_per_level,
            bonus_to_attack_rating_per_level,
            maximum_cold_damage_per_level,
            maximum_fire_damage_per_level,
            maximum_lightning_damage_per_level,
            maximum_poison_damage_per_level,
            cold_resistance_per_level,
            fire_resistance_per_level,
            lightning_resistance_per_level,
            poison_resistance_per_level,
            cold_absorption_per_level,
            fire_absorption_per_level,
            lightning_absorption_per_level,
            poison_absorption_per_level,
            thorns_per_level,
            extra_gold_per_level,
            better_chance_of_getting_magic_item_per_level,
            stamina_regeneration_per_level,
            stamina_per_level,
            damage_to_demons_per_level,
            damage_to_undead_per_level,
            attack_rating_against_demons_per_level,
            attack_rating_against_undead_per_level,
            crushing_blow_per_level,
            open_wounds_per_level,
            kick_damage_per_level,
            deadly_strike_per_level,
            find_gems_per_level,
            repairs_durability,
            replenishes_quantity,
            increased_stack_size,
            find_item,
            slash_damage,
            slash_damage_percent,
            crush_damage,
            crush_damage_percent,
            thrust_damage,
            thrust_damage_percent,
            slash_damage_absorption,
            crush_damage_absorption,
            thrust_damage_absorption,
            slash_damage_absorption_percent,
            crush_damage_absorption_percent,
            thrust_damage_absorption_percent,
            defense_per_time,
            enhanced_defense_per_time,
            life_per_time,
            mana_per_time,
            maximum_damage_per_time,
            maximum_enhanced_damage_per_time,
            strength_per_time,
            dexterity_per_time,
            energy_per_time,
            vitality_per_time,
            attack_rating_per_time,
            chance_to_hit_per_time,
            maximum_cold_damage_per_time,
            maximum_fire_damage_per_time,
            maximum_lightning_damage_per_time,
            maximum_damage_per_poison,
            cold_resistance_per_time,
            fire_resistance_per_time,
            lightning_resistance_per_time,
            poison_resistance_per_time,
            cold_absorption_per_time,
            fire_absorption_per_time,
            lightning_absorption_per_time,
            poison_absorption_per_time,
            extra_gold_per_time,
            better_chance_of_getting_magic_item_per_time,
            regenerate_stamina_per_time,
            stamina_per_time,
            damage_to_demons_per_time,
            damage_to_undead_per_time,
            attack_rating_against_demons_per_time,
            attack_rating_against_undead_per_time,
            crushing_blow_per_time,
            open_wounds_per_time,
            kick_damage_per_time,
            deadly_strike_per_time,
            find_gems_per_time,
            enemy_cold_resistance_reduction,
            enemy_fire_resistance_reduction,
            enemy_lightning_resistance_reduction,
            enemy_poison_resistance_reduction,
            damage_vs_monsters,
            enhanced_damage_vs_monsters,
            attack_rating_against_monsters,
            bonus_to_attack_rating_against_monsters,
            defense_vs_monsters,
            enhanced_defense_vs_monsters,
            fire_damage_length,
            minimum_fire_damage_length,
            maximum_fire_damage_length,
            progressive_damage,
            progressive_steal,
            progressive_other,
            progressive_fire,
            progressive_cold,
            progressive_lightning,
            extra_charges,
            progressive_attack_rating,
            poison_count,
            damage_framerate,
            pierce_idx,
            fire_mastery,
            lightning_mastery,
            cold_mastery,
            poison_mastery,
            passive_enemy_fire_resistance_reduction,
            passive_enemy_lightning_resistance_reduction,
            passive_enemy_cold_resistance_reduction,
            passive_enemy_poison_resistance_reduction,
            critical_strike,
            dodge,
            avoid,
            evade,
            warmth,
            melee_attack_rating_mastery,
            melee_damage_mastery,
            melee_critical_hit_mastery,
            thrown_weapon_attack_rating_mastery,
            thrown_weapon_damage_mastery,
            thrown_weapon_critical_hit_mastery,
            weapon_block,
            summon_resist,
            modifier_list_skill,
            modifier_list_level,
            last_sent_life_percent,
            source_unit_type,
            source_unit_id,
            short_parameter_1,
            quest_item_difficulty,
            passive_magical_damage_mastery,
            passive_magical_resistance_reduction
        };

       

    }
}
