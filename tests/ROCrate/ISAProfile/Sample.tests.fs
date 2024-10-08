module Tests.Sample

open ARCtrl.ROCrate
open DynamicObj

open TestingUtils
open Common

let mandatory_properties = Sample(
    id = "sample_mandatory_properties_id",
    name = "name"
)

let all_properties = Sample(
    id = "sample_all_properties_id",
    name = "name",
    additionalType = "additionalType",
    additionalProperty = "additionalProperty",
    derivesFrom = "derivesFrom"
)

let tests_profile_object_is_valid = testList "constructed properties" [
    testList "mandatory properties" [
        testCase "Id" <| fun _ -> Expect.ROCrateObjectHasId "sample_mandatory_properties_id" mandatory_properties
        testCase "SchemaType" <| fun _ -> Expect.ROCrateObjectHasType "bioschemas.org/Sample" mandatory_properties
        testCase "name" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "name" "name" all_properties
    ]
    testList "all properties" [
        testCase "Id" <| fun _ -> Expect.ROCrateObjectHasId "sample_all_properties_id" all_properties
        testCase "SchemaType" <| fun _ -> Expect.ROCrateObjectHasType "bioschemas.org/Sample" all_properties
        testCase "AdditionalType" <| fun _ -> Expect.ROCrateObjectHasAdditionalType "additionalType" all_properties
        testCase "name" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "name" "name" all_properties
        testCase "additionalProperty" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "additionalProperty" "additionalProperty" all_properties
        testCase "derivesFrom" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "derivesFrom" "derivesFrom" all_properties
    ]
]

let tests_interface_members = testList "interface members" [
    testCase "mandatoryProperties" <| fun _ -> Expect.ROCrateObjectHasExpectedInterfaceMembers "bioschemas.org/Sample" "sample_mandatory_properties_id" None mandatory_properties
    testCase "allProperties" <| fun _ -> Expect.ROCrateObjectHasExpectedInterfaceMembers "bioschemas.org/Sample" "sample_all_properties_id" (Some "additionalType") all_properties
]

let tests_dynamic_members = testSequenced (
    testList "dynamic members" [
        testCase "property not present before setting" <| fun _ -> Expect.isNone (DynObj.tryGetTypedValue<int> "yes" mandatory_properties) "dynamic property 'yes' was set although it was expected not to be set"
        testCase "Set dynamic property" <| fun _ ->
            mandatory_properties.SetValue("yes",42)
            Expect.ROCrateObjectHasDynamicProperty "yes" 42 mandatory_properties
        testCase "Remove dynamic property" <| fun _ ->
            mandatory_properties.Remove("yes")
            Expect.isNone (DynObj.tryGetTypedValue<int> "yes" mandatory_properties) "dynamic property 'yes' was set although it was expected not to be removed"
    ]
)

let main = testList "Sample" [
    tests_profile_object_is_valid
    tests_interface_members
    tests_dynamic_members
]