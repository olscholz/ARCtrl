module Tests.ScholarlyArticle

open ARCtrl.ROCrate
open DynamicObj

open TestingUtils
open Common

let mandatory_properties = ScholarlyArticle(
    id = "scholarlyarticle_mandatory_properties_id",
    headline = "headline",
    identifier = "identifier"
)

let all_properties = ScholarlyArticle(
    id = "scholarlyarticle_all_properties_id",
    headline = "headline",
    identifier = "identifier",
    additionalType = "additionalType",
    author = "author",
    url = "url",
    creativeWorkStatus = "creativeWorkStatus",
    disambiguatingDescription = "disambiguatingDescription"
)

let tests_profile_object_is_valid = testList "constructed properties" [
    testList "mandatory properties" [
        testCase "Id" <| fun _ -> Expect.ROCrateObjectHasId "scholarlyarticle_mandatory_properties_id" mandatory_properties
        testCase "SchemaType" <| fun _ -> Expect.ROCrateObjectHasType "schema.org/ScholarlyArticle" mandatory_properties
        testCase "headline" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "headline" "headline" all_properties
        testCase "identifier" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "identifier" "identifier" all_properties
    ]
    testList "all properties" [
        testCase "Id" <| fun _ -> Expect.ROCrateObjectHasId "scholarlyarticle_mandatory_properties_id" mandatory_properties
        testCase "SchemaType" <| fun _ -> Expect.ROCrateObjectHasType "schema.org/ScholarlyArticle" mandatory_properties
        testCase "AdditionalType" <| fun _ -> Expect.ROCrateObjectHasAdditionalType "additionalType" all_properties
        testCase "headline" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "headline" "headline" all_properties
        testCase "identifier" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "identifier" "identifier" all_properties
        testCase "author" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "author" "author" all_properties
        testCase "url" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "url" "url" all_properties
        testCase "creativeWorkStatus" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "creativeWorkStatus" "creativeWorkStatus" all_properties
        testCase "disambiguatingDescription" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "disambiguatingDescription" "disambiguatingDescription" all_properties
    ]
]

let tests_interface_members = testList "interface members" [
    testCase "mandatoryProperties" <| fun _ -> Expect.ROCrateObjectHasExpectedInterfaceMembers "schema.org/ScholarlyArticle" "scholarlyarticle_mandatory_properties_id" None mandatory_properties
    testCase "allProperties" <| fun _ -> Expect.ROCrateObjectHasExpectedInterfaceMembers "schema.org/ScholarlyArticle" "scholarlyarticle_all_properties_id" (Some "additionalType") all_properties
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

let main = testList "ScholarlyArticle" [
    tests_profile_object_is_valid
    tests_interface_members
    tests_dynamic_members
]