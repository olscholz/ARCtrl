module Tests.Study

open ARCtrl.ROCrate
open DynamicObj

open TestingUtils
open Common

let mandatory_properties = Study(
    id = "study_mandatory_properties_id",
    identifier = "identifier"
)

let all_properties = Study(
    id = "study_all_properties_id",
    identifier = "identifier",
    about = "about",
    citation = "citation",
    comment = "comment",
    creator = "creator",
    dateCreated = "dateCreated",
    dateModified = "dateModified",
    datePublished = "datePublished",
    description = "description",
    hasPart = "hasPart",
    headline = "headline",
    url = "url"
)

let tests_profile_object_is_valid = testList "constructed properties" [
    testList "mandatory properties" [
        testCase "Id" <| fun _ -> Expect.ROCrateObjectHasId "study_mandatory_properties_id" mandatory_properties
        testCase "SchemaType" <| fun _ -> Expect.ROCrateObjectHasType "schema.org/Dataset" mandatory_properties
        testCase "AdditionalType" <| fun _ -> Expect.ROCrateObjectHasAdditionalType "Study" mandatory_properties
        testCase "identifier" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "identifier" "identifier" mandatory_properties
    ]
    testList "all properties" [
        testCase "Id" <| fun _ -> Expect.ROCrateObjectHasId "study_all_properties_id" all_properties
        testCase "SchemaType" <| fun _ -> Expect.ROCrateObjectHasType "schema.org/Dataset" all_properties
        testCase "AdditionalType" <| fun _ -> Expect.ROCrateObjectHasAdditionalType "Study" all_properties
        testCase "identifier" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "identifier" "identifier" all_properties
        testCase "about" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "about" "about" all_properties
        testCase "citation" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "citation" "citation" all_properties
        testCase "comment" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "comment" "comment" all_properties
        testCase "creator" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "creator" "creator" all_properties
        testCase "dateCreated" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "dateCreated" "dateCreated" all_properties
        testCase "dateModified" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "dateModified" "dateModified" all_properties
        testCase "datePublished" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "datePublished" "datePublished" all_properties
        testCase "description" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "description" "description" all_properties
        testCase "hasPart" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "hasPart" "hasPart" all_properties
        testCase "headline" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "headline" "headline" all_properties
        testCase "url" <| fun _ -> Expect.ROCrateObjectHasDynamicProperty "url" "url" all_properties
    ]
]

let tests_interface_members = testList "interface members" [
    testCase "mandatoryProperties" <| fun _ -> Expect.ROCrateObjectHasExpectedInterfaceMembers "schema.org/Dataset" "study_mandatory_properties_id" (Some "Study") mandatory_properties
    testCase "allProperties" <| fun _ -> Expect.ROCrateObjectHasExpectedInterfaceMembers "schema.org/Dataset" "study_all_properties_id" (Some "Study") all_properties
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

let main = testList "Study" [
    tests_profile_object_is_valid
    tests_interface_members
    tests_dynamic_members
]