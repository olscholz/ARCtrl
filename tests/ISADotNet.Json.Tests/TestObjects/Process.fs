﻿module TestObjects.Process

let process' = 

    """
    {
    "@id": "#process/standard_trypsin_digestion",
    "name": "standard_trypsin_digestion",
    "executesProtocol": {
        "@id": "#protocols/peptide_digestion",
        "name": "peptide_digestion",
    
        "protocolType": {
            "@id": "protein_digestion",
            "annotationValue": "Protein Digestion",
            "termSource": "NCIT",
            "termAccession": "http://purl.obolibrary.org/obo/NCIT_C70845",
            "comments": []
        },
        "description": "The isolated proteins get solubilized. Given protease is added and the solution is heated to a given temperature. After a given amount of time, the digestion is stopped by adding a denaturation agent.",
        "uri": "http://madeUpProtocolWebsize.org/protein_digestion",
        "version": "1.0.0",
        "parameters": [
            {
                "@id": "protease",
                "parameterName": {
                    "@id": "protease",
                    "annotationValue": "Peptidase",
                    "termSource": "MS",
                    "termAccession": "http://purl.obolibrary.org/obo/NCIT_C16965",
                    "comments": []
                }
            },
            {
                "@id": "temperature",
                "parameterName": {
                    "@id": "temperature",
                    "annotationValue": "temperature",
                    "termSource": "Ontobee",
                    "termAccession": "http://purl.obolibrary.org/obo/NCRO_0000029",
                    "comments": []
    
                }
            },
            {
                "@id": "time",
                "parameterName": {
                    "@id": "time",
                    "annotationValue": "time",
                    "termSource": "EFO",
                    "termAccession": "http://www.ebi.ac.uk/efo/EFO_0000721",
                    "comments": []
                }
            }
        ],
        "components": [
            {
                "componentName": "digestion_stopper",
                "componentType": {
                    "@id": "formic_acid",
                    "annotationValue": "Formic Acid",
                    "termSource": "NCIT",
                    "termAccession": "http://purl.obolibrary.org/obo/NCIT_C83719",
                    "comments": []
                }
            },
            {
                "componentName": "heater",
                "componentType": {
                    "@id": "heater",
                    "annotationValue": "Heater Device",
                    "termSource": "NCIT",
                    "termAccession": "http://purl.obolibrary.org/obo/NCIT_C49986",
                    "comments": []
                }
            }
            
        ],
        "comments": []
    },
    "parameterValues": [
        {
            "category": {
                "@id": "protease",
                "parameterName": {
                    "@id": "protease",
                    "annotationValue": "Peptidase",
                    "termSource": "MS",
                    "termAccession": "http://purl.obolibrary.org/obo/NCIT_C16965",
                    "comments": []
                }
            },
            "value": {
                "@id": "trypsin",
                "annotationValue": "Trypsin/P",
                "termSource": "NCI",
                "termAccession": "http://purl.obolibrary.org/obo/MS_1001313",
                "comments": []
                
            }
        },
        {
            "category": {
                "@id": "temperature",
                "parameterName": {
                    "@id": "temperature",
                    "annotationValue": "temperature",
                    "termSource": "Ontobee",
                    "termAccession": "http://purl.obolibrary.org/obo/NCRO_0000029",
                    "comments": []

                }
            },
            "value": 37,
            "unit": {
                "@id": "degree_celcius",
                "annotationValue": "degree Celsius",
                "termSource": "OM2",
                "termAccession": "http://www.ontology-of-units-of-measure.org/resource/om-2/degreeCelsius",
                "comments": []
            }
        },
        {
            "category": {
                "@id": "time",
                "parameterName": {
                    "@id": "time",
                    "annotationValue": "time",
                    "termSource": "EFO",
                    "termAccession": "http://www.ebi.ac.uk/efo/EFO_0000721",
                    "comments": []
                }
            },
            "value": 1,
            "unit": {
                "@id": "h",
                "annotationValue": "hour",
                "termSource": "UO",
                "termAccession": "http://purl.obolibrary.org/obo/UO_0000032",
                "comments": []
            }
        }
    ],
    "date": "2020-10-23",
    "performer": "TUKL",
    "previousProcess": { "@id": "#process/protein_extraction" },
    "nextProcess":  { "@id": "#process/massspec_measurement"},
    "inputs": [
        {
            "@id": "#sample/WT_protein"
        },
        {
            "@id": "#sample/MUT1_protein"
        },
        {
            "@id": "#sample/MUT2_protein"
        }
    ],
    "outputs": [
        {
            "@id": "#sample/WT_digested"
        },
        {
            "@id": "#sample/MUT1_digested"
        },
        {
            "@id": "#sample/MUT2_digested"
        }
    ],
    "comments": []

}
    """