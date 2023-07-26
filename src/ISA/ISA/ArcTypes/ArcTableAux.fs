﻿module ISA.ArcTableAux

open ISA
open System.Collections.Generic

// Taken from FSharpAux.Core
/// .Net Dictionary
module Dictionary = 
    
    /// <summary>Returns the dictionary with the binding added to the given dictionary.
    /// If a binding with the given key already exists in the input dictionary, the existing binding is replaced by the new binding in the result dictionary.</summary>
    /// <param name="key">The input key.</param>
    /// <returns>The dictionary with change in place.</returns>
    let addOrUpdateInPlace key value (table:IDictionary<_,_>) =
        match table.ContainsKey(key) with
        | true  -> table.[key] <- value
        | false -> table.Add(key,value)
        table

    /// <summary>Lookup an element in the dictionary, returning a <c>Some</c> value if the element is in the domain 
    /// of the dictionary and <c>None</c> if not.</summary>
    /// <param name="key">The input key.</param>
    /// <returns>The mapped value, or None if the key is not in the dictionary.</returns>
    let tryFind key (table:IDictionary<_,_>) =
        match table.ContainsKey(key) with
        | true -> Some table.[key]
        | false -> None

let getColumnCount (headers:ResizeArray<CompositeHeader>) = 
    headers.Count

let getRowCount (values:Dictionary<int*int,CompositeCell>) = 
    if values.Count = 0 then 0 else
        values.Keys |> Seq.maxBy snd |> snd |> (+) 1

// TODO: Move to CompositeHeader?
let (|IsUniqueExistingHeader|_|) existingHeaders (input: CompositeHeader) = 
    match input with
    | CompositeHeader.Parameter _
    | CompositeHeader.Factor _
    | CompositeHeader.Characteristic _
    | CompositeHeader.Component _
    | CompositeHeader.FreeText _        -> None
    // Input and Output does not look very clean :/
    | CompositeHeader.Output _          -> Seq.tryFindIndex (fun h -> match h with | CompositeHeader.Output _ -> true | _ -> false) existingHeaders
    | CompositeHeader.Input _           -> Seq.tryFindIndex (fun h -> match h with | CompositeHeader.Input _ -> true | _ -> false) existingHeaders
    | header                            -> Seq.tryFindIndex (fun h -> h = header) existingHeaders
        
// TODO: Move to CompositeHeader?
/// Returns the column index of the duplicate unique column in `existingHeaders`.
let tryFindDuplicateUnique (newHeader: CompositeHeader) (existingHeaders: seq<CompositeHeader>) = 
    match newHeader with
    | IsUniqueExistingHeader existingHeaders index -> Some index
    | _ -> None

/// Returns the column index of the duplicate unique column in `existingHeaders`.
let tryFindDuplicateUniqueInArray (existingHeaders: seq<CompositeHeader>) = 
    let rec loop i (duplicateList: {|Index1: int; Index2: int; HeaderType: CompositeHeader|} list) (headerList: CompositeHeader list) =
        match headerList with
        | _ :: [] | [] -> duplicateList
        | header :: tail -> 
            let hasDuplicate = tryFindDuplicateUnique header tail
            let nextDuplicateList = if hasDuplicate.IsSome then {|Index1 = i; Index2 = hasDuplicate.Value; HeaderType = header|}::duplicateList else duplicateList
            loop (i+1) nextDuplicateList tail
    existingHeaders
    |> Seq.filter (fun x -> not x.IsTermColumn)
    |> List.ofSeq
    |> loop 0 []

module SanityChecks =

    /// Checks if given column index is valid for given number of columns.
    ///
    /// if `allowAppend` = true => `0 < index <= columnCount`
    /// 
    /// if `allowAppend` = false => `0 < index < columnCount`
    let validateColumnIndex (index:int) (columnCount:int) (allowAppend:bool) =
        let eval x y = if allowAppend then x > y else x >= y
        if index < 0 then failwith "Cannot insert CompositeColumn at index < 0."
        if eval index columnCount then failwith $"Specified index is out of table range! Table contains only {columnCount} columns."

    /// Checks if given index is valid for given number of rows.
    ///
    /// if `allowAppend` = true => `0 < index <= rowCount`
    /// 
    /// if `allowAppend` = false => `0 < index < rowCount`
    let validateRowIndex (index:int) (rowCount:int) (allowAppend:bool) =
        let eval x y = if allowAppend then x > y else x >= y
        if index < 0 then failwith "Cannot insert CompositeColumn at index < 0."
        if eval index rowCount then failwith $"Specified index is out of table range! Table contains only {rowCount} rows."

    let validateColumn (column:CompositeColumn) = column.validate(true) |> ignore

    let inline validateNoDuplicateUniqueColumns (columns:seq<CompositeColumn>) =
        let duplicates = columns |> Seq.map (fun x -> x.Header) |> tryFindDuplicateUniqueInArray
        if not <| List.isEmpty duplicates then
            let baseMsg = "Found duplicate unique columns in `columns`."
            let sb = System.Text.StringBuilder()
            sb.AppendLine(baseMsg) |> ignore
            duplicates |> List.iter (fun (x: {| HeaderType: CompositeHeader; Index1: int; Index2: int |}) -> 
                sb.AppendLine($"Duplicate `{x.HeaderType}` at index {x.Index1} and {x.Index2}.")
                |> ignore
            )
            let msg = sb.ToString()
            failwith msg

    let inline validateNoDuplicateUnique (header: CompositeHeader) (columns:seq<CompositeHeader>) =
        match tryFindDuplicateUnique header columns with
        | None -> ()
        | Some i -> failwith $"Invalid input. Tried setting unique header `{header}`, but header of same type already exists at index {i}."

    let inline validateRowLength (newCells: seq<CompositeCell>) (columnCount: int) =
        let newCellsCount = newCells |> Seq.length
        match columnCount with
        | 0 ->
            failwith $"Table contains no columns! Cannot add row to empty table!"
        | unequal when newCellsCount <> columnCount ->
            failwith $"Cannot add a new row with {newCellsCount} cells, as the table has {columnCount} columns."
        | _ ->
            ()

module Unchecked =
        
    let tryGetCellAt (column: int,row: int) (cells:System.Collections.Generic.Dictionary<int*int,CompositeCell>) = Dictionary.tryFind (column, row) cells
    let setCellAt(columnIndex, rowIndex,c : CompositeCell) (cells:Dictionary<int*int,CompositeCell>) = Dictionary.addOrUpdateInPlace (columnIndex,rowIndex) c cells |> ignore
    let moveCellTo (fromCol:int,fromRow:int,toCol:int,toRow:int) (cells:Dictionary<int*int,CompositeCell>) =
        match Dictionary.tryFind (fromCol, fromRow) cells with
        | Some c ->
            // Dictionary.addOrUpdateInPlace (column+amount,row) c this.Values |> ignore // This is the same as `SetCellAt`
            // Remove value. This is necessary in the following scenario:
            //
            // "AddColumn.Existing Table.add less rows, insert at".
            //
            // Assume a table with 5 rows, insert column with 2 cells. All 5 rows at `index` position are shifted +1, but only row 0 and 1 are replaced with new values.
            // Without explicit removing, row 2..4 would stay as is.
            // EDIT: First remove then set cell to otherwise a `amount` = 0 would just remove the cell!
            cells.Remove((fromCol,fromRow)) |> ignore
            setCellAt(toCol,toRow,c) cells
            |> ignore
        | None -> ()
    let removeHeader (index:int) (headers:ResizeArray<CompositeHeader>) = headers.RemoveAt (index)
    /// Remove cells of one Column, change index of cells with higher index to index - 1
    let removeColumnCells (index:int) (cells:Dictionary<int*int,CompositeCell>) = 
        for KeyValue((c,r),_) in cells do
            // Remove cells of column
            if c = index then
                cells.Remove((c,r))
                |> ignore
            else
                ()
    /// Remove cells of one Column, change index of cells with higher index to index - 1
    let removeColumnCells_withIndexChange (index:int) (columnCount:int) (rowCount:int) (cells:Dictionary<int*int,CompositeCell>) = 
        // Cannot loop over collection and change keys of existing.
        // Therefore we need to run over values between columncount and rowcount.
        for col = index to (columnCount-1) do
            for row = 0 to (rowCount-1) do
                if col = index then
                    cells.Remove((col,row))
                    |> ignore
                // move to left if "column index > index"
                elif col > index then
                    moveCellTo(col,row,col-1,row) cells
                else
                    ()
    let removeRowCells (rowIndex:int) (cells:Dictionary<int*int,CompositeCell>) = 
        for KeyValue((c,r),_) in cells do
            // Remove cells of column
            if r = rowIndex then
                cells.Remove((c,r))
                |> ignore
            else
                ()
    /// Remove cells of one Row, change index of cells with higher index to index - 1
    let removeRowCells_withIndexChange (rowIndex:int) (columnCount:int) (rowCount:int) (cells:Dictionary<int*int,CompositeCell>) = 
        // Cannot loop over collection and change keys of existing.
        // Therefore we need to run over values between columncount and rowcount.
        for row = rowIndex to (rowCount-1) do
            for col = 0 to (columnCount-1) do
                if row = rowIndex then
                    cells.Remove((col,row))
                    |> ignore
                // move to top if "row index > index"
                elif row > rowIndex then
                    moveCellTo(col,row,col,row-1) cells
                else
                    ()

    /// Get an empty cell fitting for related column header.
    ///
    /// `columCellOption` is used to decide between `CompositeCell.Term` or `CompositeCell.Unitized`. `columCellOption` can be any other cell in the same column, preferably the first one.
    let getEmptyCellForHeader (header:CompositeHeader) (columCellOption: CompositeCell option) =
        match header.IsTermColumn with
        | false                                 -> CompositeCell.emptyFreeText
        | true ->
            match columCellOption with
            | Some (CompositeCell.Term _) 
            | None                              -> CompositeCell.emptyTerm
            | Some (CompositeCell.Unitized _)   -> CompositeCell.emptyUnitized
            | _                                 -> failwith "[extendBodyCells] This should never happen, IsTermColumn header must be paired with either term or unitized cell."

    let addColumn (newHeader: CompositeHeader) (newCells: CompositeCell []) (index: int) (forceReplace: bool) (headers: ResizeArray<CompositeHeader>) (values:Dictionary<int*int,CompositeCell>) =
        let mutable numberOfNewColumns = 1
        let mutable index = index
        /// If this isSome and the function does not raise exception we are executing a forceReplace.
        let hasDuplicateUnique = tryFindDuplicateUnique newHeader headers
        // implement fail if unique column should be added but exists already
        if not forceReplace && hasDuplicateUnique.IsSome then failwith $"Invalid new column `{newHeader}`. Table already contains header of the same type on index `{hasDuplicateUnique.Value}`"
        // Example: existingCells contains `Output io` (With io being any IOType) and header is `Output RawDataFile`. This should replace the existing `Output io`.
        // In this case the number of new columns drops to 0 and we insert the index of the existing `Output io` column.
        if hasDuplicateUnique.IsSome then
            numberOfNewColumns <- 0
            index <- hasDuplicateUnique.Value
        /// This ensures nothing gets messed up during mutable insert, for example inser header first and change ColumCount in the process
        let startColCount, startRowCount = getColumnCount headers, getRowCount values
        // headers are easily added. Just insert at position of index. This will insert without replace.
        let setNewHeader = 
            // if duplication found and we want to forceReplace we remove related header
            if hasDuplicateUnique.IsSome then
                removeHeader(index) headers
            headers.Insert(index, newHeader)
        /// For all columns with index >= we need to increase column index by `numberOfNewColumns`.
        /// We do this by moving all these columns one to the right with mutable dictionary set logic (cells.[key] <- newValue), 
        /// Therefore we need to start with the last column to not overwrite any values we still need to shift
        let increaseColumnIndices =
            // Only do this if column is inserted and not appended AND we do not execute forceReplace!
            if index < startColCount && hasDuplicateUnique.IsNone then
                /// Get last column index
                let lastColumnIndex = System.Math.Max(startColCount - 1, 0) // If there are no columns. We get negative last column index. In this case just return 0.
                // start with last column index and go down to `index`
                for columnIndex = lastColumnIndex downto index do
                    for rowIndex in 0 .. startRowCount do
                        moveCellTo(columnIndex,rowIndex,columnIndex+numberOfNewColumns,rowIndex) values
        /// Then we can set the new column at `index`
        let setNewCells =
            // Not sure if this is intended? If we for example `forceReplace` a single column table with `Input`and 5 rows with a new column of `Input` ..
            // ..and only 2 rows, then table RowCount will decrease from 5 to 2.
            // Related Test: `All.ArcTable.addColumn.Existing Table.add less rows, replace input, force replace
            if hasDuplicateUnique.IsSome then
                removeColumnCells(index) values
            newCells |> Array.iteri (fun rowIndex cell ->
                let columnIndex = index
                setCellAt (columnIndex,rowIndex,cell) values
            )
        ()

    // We need to calculate the max number of rows between the new columns and the existing columns in the table.
    // `maxRows` will be the number of rows all columns must have after adding the new columns.
    // This behaviour should be intuitive for the user, as Excel handles this case in the same way.
    let fillMissingCells (headers: ResizeArray<CompositeHeader>) (values:Dictionary<int*int,CompositeCell>) =
        let rowCount = getRowCount values
        let columnCount = getColumnCount headers
        let maxRows = rowCount
        let lastColumnIndex = columnCount - 1
        /// Get all keys, to map over relevant rows afterwards
        let keys = values.Keys
        // iterate over columns
        for columnIndex in 0 .. lastColumnIndex do
            /// Only get keys for the relevant column
            let colKeys = keys |> Seq.filter (fun (c,_) -> c = columnIndex) |> Set.ofSeq 
            /// Create set of expected keys
            let expectedKeys = Seq.init maxRows (fun i -> columnIndex,i) |> Set.ofSeq 
            /// Get the missing keys
            let missingKeys = Set.difference expectedKeys colKeys 
            // if no missing keys, we are done and skip the rest, if not empty missing keys we ...
            if missingKeys.IsEmpty |> not then
                /// .. first check which empty filler `CompositeCells` we need. 
                ///
                /// We use header to decide between CompositeCell.Term/CompositeCell.Unitized and CompositeCell.FreeText
                let relatedHeader = headers.[columnIndex]
                /// We use the first cell in the column to decide between CompositeCell.Term and CompositeCell.Unitized
                ///
                /// Not sure if we can add a better logic to infer if empty cells should be term or unitized ~Kevin F
                let tryExistingCell = if colKeys.IsEmpty then None else Some values.[colKeys.MinimumElement]
                let empty = getEmptyCellForHeader relatedHeader tryExistingCell
                for missingColumn,missingRow in missingKeys do
                    setCellAt (missingColumn,missingRow,empty) values

    let addRow (index:int) (newCells:CompositeCell []) (headers: ResizeArray<CompositeHeader>) (values:Dictionary<int*int,CompositeCell>) =
        /// Store start rowCount here, so it does not get changed midway through
        let rowCount = getRowCount values
        let columnCount = getColumnCount headers
        let increaseRowIndices =  
            // Only do this if column is inserted and not appended!
            if index < rowCount then
                /// Get last row index
                let lastRowIndex = System.Math.Max(rowCount - 1, 0) // If there are no rows. We get negative last column index. In this case just return 0.
                // start with last row index and go down to `index`
                for rowIndex = lastRowIndex downto index do
                    for columnIndex in 0 .. (columnCount-1) do
                        moveCellTo(columnIndex,rowIndex,columnIndex,rowIndex+1) values
        /// Then we can set the new row at `index`
        let setNewCells =
            newCells |> Array.iteri (fun columnIndex cell ->
                let rowIndex = index
                setCellAt (columnIndex,rowIndex,cell) values
            )
        ()

module JsonTypes = 

    let valueOfCell (value : CompositeCell) =
        match value with
        | CompositeCell.FreeText (text) -> Value.fromString text, None
        | CompositeCell.Term (term) -> Value.Ontology term, None
        | CompositeCell.Unitized (text,unit) -> Value.fromString text, Some unit

    let composeComponent (header : CompositeHeader) (value : CompositeCell) : Component =
        let v,u = valueOfCell value
        Component.fromOptions (Some v) u (header.ToTerm() |> Some)

    let composeParameterValue (header : CompositeHeader) (value : CompositeCell) : ProcessParameterValue =
        let v,u = valueOfCell value
        let p = ProtocolParameter.create(ParameterName = header.ToTerm())
        ProcessParameterValue.create(p,v,?Unit = u)

    let composeFactorValue (header : CompositeHeader) (value : CompositeCell) : FactorValue =
        let v,u = valueOfCell value
        let f = Factor.create(Name = header.ToString(),FactorType = header.ToTerm())
        FactorValue.create(Category = f,Value = v,?Unit = u)

    let composeCharacteristicValue (header : CompositeHeader) (value : CompositeCell) : MaterialAttributeValue =
        let v,u = valueOfCell value
        let c = MaterialAttribute.create(CharacteristicType = header.ToTerm())
        MaterialAttributeValue.create(Category = c,Value = v,?Unit = u)

    let composeProcessInput (header : CompositeHeader) (value : CompositeCell) : ProcessInput =
        match header with
        | CompositeHeader.Input IOType.Source -> 
            Source.create(Name = value.ToString())
            |> ProcessInput.Source
        | CompositeHeader.Input IOType.Sample ->
            Sample.create(Name = value.ToString())
            |> ProcessInput.Sample
        | CompositeHeader.Input IOType.Material ->
            Material.create(Name = value.ToString())
            |> ProcessInput.Material
        | CompositeHeader.Input IOType.ImageFile -> 
            Data.create(Name = value.ToString(),DataType = DataFile.ImageFile)
            |> ProcessInput.Data

        | CompositeHeader.Input IOType.RawDataFile -> 
            Data.create(Name = value.ToString(),DataType = DataFile.RawDataFile)
            |> ProcessInput.Data
        | CompositeHeader.Input IOType.DerivedDataFile ->
            Data.create(Name = value.ToString(),DataType = DataFile.DerivedDataFile)
            |> ProcessInput.Data
        | _ ->
            failwithf "Could not parse input header %O" header

    let composeProcessOutput (header : CompositeHeader) (value : CompositeCell) : ProcessOutput =
        match header with
        | CompositeHeader.Output IOType.Sample ->
            Sample.create(Name = value.ToString())
            |> ProcessOutput.Sample
        | CompositeHeader.Output IOType.Material ->
            Material.create(Name = value.ToString())
            |> ProcessOutput.Material
        | CompositeHeader.Output IOType.ImageFile ->
            Data.create(Name = value.ToString(),DataType = DataFile.ImageFile)
            |> ProcessOutput.Data
        | CompositeHeader.Output IOType.RawDataFile ->
            Data.create(Name = value.ToString(),DataType = DataFile.RawDataFile)
            |> ProcessOutput.Data
        | CompositeHeader.Output IOType.DerivedDataFile ->
            Data.create(Name = value.ToString(),DataType = DataFile.DerivedDataFile)
            |> ProcessOutput.Data
        | _ ->
            failwithf "Could not parse output header %O" header

    let cellOfValue (value : Value option) (unit : OntologyAnnotation option) =
        let value = value |> Option.defaultValue (Value.Name "")
        match value,unit with
        | Value.Ontology oa, None -> CompositeCell.Term oa
        | Value.Name text, None -> CompositeCell.FreeText text
        | Value.Name "", Some u -> CompositeCell.Unitized ("",u)
        | Value.Float f, Some u -> CompositeCell.Unitized (f.ToString(),u)
        | Value.Float f, None -> CompositeCell.FreeText (f.ToString())
        | Value.Int i, Some u -> CompositeCell.Unitized (i.ToString(),u)
        | Value.Int i, None -> CompositeCell.FreeText (i.ToString())
        | _ -> failwithf "Could not parse value %O with unit %O" value unit

    let decomposeComponent (c : Component) : CompositeHeader*CompositeCell =
        CompositeHeader.Component (c.ComponentType.Value),
        cellOfValue c.ComponentValue c.ComponentUnit 

    let decomposeParameterValue (ppv : ProcessParameterValue) : CompositeHeader*CompositeCell =
        CompositeHeader.Parameter (ppv.Category.Value.ParameterName.Value),
        cellOfValue ppv.Value ppv.Unit

    let decomposeFactorValue (fv : FactorValue) : CompositeHeader*CompositeCell =
        CompositeHeader.Parameter (fv.Category.Value.FactorType.Value),
        cellOfValue fv.Value fv.Unit

    let decomposeCharacteristicValue (cv : MaterialAttributeValue) : CompositeHeader*CompositeCell =
        CompositeHeader.Parameter (cv.Category.Value.CharacteristicType.Value),
        cellOfValue cv.Value cv.Unit

    let decomposeProcessInput (pi : ProcessInput) : CompositeHeader*CompositeCell =
        match pi with
        | ProcessInput.Source s -> CompositeHeader.Input IOType.Source, CompositeCell.FreeText (s.Name |> Option.defaultValue "")
        | ProcessInput.Sample s -> CompositeHeader.Input IOType.Sample, CompositeCell.FreeText (s.Name |> Option.defaultValue "")
        | ProcessInput.Material m -> CompositeHeader.Input IOType.Material, CompositeCell.FreeText (m.Name |> Option.defaultValue "")
        | ProcessInput.Data d -> 
            let dataType = d.DataType.Value
            match dataType with
            | DataFile.ImageFile -> CompositeHeader.Input IOType.ImageFile, CompositeCell.FreeText (d.Name |> Option.defaultValue "")
            | DataFile.RawDataFile -> CompositeHeader.Input IOType.RawDataFile, CompositeCell.FreeText (d.Name |> Option.defaultValue "")
            | DataFile.DerivedDataFile -> CompositeHeader.Input IOType.DerivedDataFile, CompositeCell.FreeText (d.Name |> Option.defaultValue "")

    let decomposeProcessOutput (po : ProcessOutput) : CompositeHeader*CompositeCell =
        match po with
        | ProcessOutput.Sample s -> CompositeHeader.Output IOType.Sample, CompositeCell.FreeText (s.Name |> Option.defaultValue "")
        | ProcessOutput.Material m -> CompositeHeader.Output IOType.Material, CompositeCell.FreeText (m.Name |> Option.defaultValue "")
        | ProcessOutput.Data d -> 
            let dataType = d.DataType.Value
            match dataType with
            | DataFile.ImageFile -> CompositeHeader.Output IOType.ImageFile, CompositeCell.FreeText (d.Name |> Option.defaultValue "")
            | DataFile.RawDataFile -> CompositeHeader.Output IOType.RawDataFile, CompositeCell.FreeText (d.Name |> Option.defaultValue "")
            | DataFile.DerivedDataFile -> CompositeHeader.Output IOType.DerivedDataFile, CompositeCell.FreeText (d.Name |> Option.defaultValue "")

module ProcessParsing = 

    open ISA.ColumnIndex

    /// If the headers of a node depict a component, returns a function for parsing the values of the matrix to the values of this component
    let tryComponentGetter (generalI : int) (valueI : int) (valueHeader : CompositeHeader) =
        match valueHeader with
        | CompositeHeader.Component oa ->
            let cat = CompositeHeader.Component (oa.SetColumnIndex valueI)
            fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                JsonTypes.composeComponent cat matrix.[generalI,i]
            |> Some
        | _ -> None    
            
    /// If the headers of a node depict a protocolType, returns a function for parsing the values of the matrix to the values of this type
    let tryParameterGetter (generalI : int) (valueI : int) (valueHeader : CompositeHeader) =
        match valueHeader with
        | CompositeHeader.Component oa ->
            let cat = CompositeHeader.Component (oa.SetColumnIndex valueI)
            fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                JsonTypes.composeParameterValue cat matrix.[generalI,i]
            |> Some
        | _ -> None 

    let tryFactorGetter (generalI : int) (valueI : int) (valueHeader : CompositeHeader) =
        match valueHeader with
        | CompositeHeader.Component oa ->
            let cat = CompositeHeader.Component (oa.SetColumnIndex valueI)
            fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                JsonTypes.composeFactorValue cat matrix.[generalI,i]
            |> Some
        | _ -> None 

    let tryCharacteristicGetter (generalI : int) (valueI : int) (valueHeader : CompositeHeader) =
        match valueHeader with
        | CompositeHeader.Component oa ->
            let cat = CompositeHeader.Component (oa.SetColumnIndex valueI)
            fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                JsonTypes.composeCharacteristicValue cat matrix.[generalI,i]
            |> Some
        | _ -> None 

    /// If the headers of a node depict a protocolType, returns a function for parsing the values of the matrix to the values of this type
    let tryGetProtocolTypeGetter (generalI : int) (header : CompositeHeader) =
        match header with
        | CompositeHeader.ProtocolType ->
            fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                matrix.[generalI,i].AsTerm
            |> Some
        | _ -> None 


    let tryGetProtocolREFGetter (generalI : int) (header : CompositeHeader) =
        match header with
        | CompositeHeader.ProtocolREF ->
            fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                matrix.[generalI,i].AsFreeText
            |> Some
        | _ -> None

    let tryGetProtocolDescriptionGetter (generalI : int) (header : CompositeHeader) =
        match header with
        | CompositeHeader.ProtocolDescription ->
            fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                matrix.[generalI,i].AsFreeText
            |> Some
        | _ -> None

    let tryGetProtocolURIGetter (generalI : int) (header : CompositeHeader) =
        match header with
        | CompositeHeader.ProtocolUri ->
            fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                matrix.[generalI,i].AsFreeText
            |> Some
        | _ -> None

    let tryGetProtocolVersionGetter (generalI : int) (header : CompositeHeader) =
        match header with
        | CompositeHeader.ProtocolVersion ->
            fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                matrix.[generalI,i].AsFreeText
            |> Some
        | _ -> None

    let tryGetInputGetter (generalI : int) (header : CompositeHeader) =
        match header with
        | CompositeHeader.Input io ->
            fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                JsonTypes.composeProcessInput header matrix.[generalI,i]
            |> Some
        | _ -> None

    let tryGetOutputGetter (generalI : int) (header : CompositeHeader) =
        match header with
        | CompositeHeader.Output io ->
            fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                JsonTypes.composeProcessOutput header matrix.[generalI,i]
            |> Some
        | _ -> None

    /// Returns the protocol described by the headers and a function for parsing the values of the matrix to the processes of this protocol
    let getProcessGetter (processNameRoot : string) (headers : CompositeHeader seq) =
    
        let headers = 
            headers
            |> Seq.indexed

        let valueHeaders =
            headers
            |> Seq.filter (snd >> fun h -> h.IsCvParamColumn)
            |> Seq.indexed
            |> Seq.toList

        let charGetters =
            valueHeaders 
            |> List.choose (fun (valueI,(generalI,header)) -> tryCharacteristicGetter generalI valueI header)

        let factorValueGetters =
            valueHeaders
            |> List.choose (fun (valueI,(generalI,header)) -> tryFactorGetter generalI valueI header)

        let parameterValueGetters =
            valueHeaders
            |> List.choose (fun (valueI,(generalI,header)) -> tryParameterGetter generalI valueI header)

        let componentGetters =
            valueHeaders
            |> List.choose (fun (valueI,(generalI,header)) -> tryComponentGetter generalI valueI header)

        let protocolTypeGetter = 
            headers
            |> Seq.tryPick (fun (generalI,header) -> tryGetProtocolTypeGetter generalI header)

        let protocolREFGetter = 
            headers
            |> Seq.tryPick (fun (generalI,header) -> tryGetProtocolREFGetter generalI header)

        let protocolDescriptionGetter = 
            headers
            |> Seq.tryPick (fun (generalI,header) -> tryGetProtocolDescriptionGetter generalI header)

        let protocolURIGetter = 
            headers
            |> Seq.tryPick (fun (generalI,header) -> tryGetProtocolURIGetter generalI header)

        let protocolVersionGetter =
            headers
            |> Seq.tryPick (fun (generalI,header) -> tryGetProtocolVersionGetter generalI header)

        let inputGetter =
            match headers |> Seq.tryPick (fun (generalI,header) -> tryGetInputGetter generalI header) with
            | Some inputGetter ->
                fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                    let chars = charGetters |> Seq.map (fun f -> f matrix i) |> Seq.toList
                    inputGetter matrix i
                    |> ProcessInput.setCharacteristicValues chars
                    |> List.singleton
            | None ->
                fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                    let chars = charGetters |> Seq.map (fun f -> f matrix i) |> Seq.toList
                    ProcessInput.Source (Source.create(Name = $"{processNameRoot}_Input_{i}", Characteristics = chars))
                    |> List.singleton
            
        let outputGetter =
            match headers |> Seq.tryPick (fun (generalI,header) -> tryGetOutputGetter generalI header) with
            | Some outputGetter ->
                fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                    let factors = factorValueGetters |> Seq.map (fun f -> f matrix i) |> Seq.toList
                    outputGetter matrix i
                    |> ProcessOutput.setFactorValues factors
                    |> List.singleton
            | None ->
                fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->
                    let factors = factorValueGetters |> Seq.map (fun f -> f matrix i) |> Seq.toList
                    ProcessOutput.Sample (Sample.create(Name = $"{processNameRoot}_Output_{i}", FactorValues = factors))
                    |> List.singleton

        fun (matrix : System.Collections.Generic.Dictionary<(int * int),CompositeCell>) i ->

            let pn = processNameRoot |> Aux.Option.fromValueWithDefault ""

            let paramvalues = parameterValueGetters |> List.map (fun f -> f matrix i) |> Aux.Option.fromValueWithDefault [] 
            let parameters = paramvalues |> Option.map (List.map (fun pv -> pv.Category.Value))

            let protocol : Protocol = 
                Protocol.make 
                    None
                    (protocolREFGetter |> Aux.Option.mapOrDefault pn (fun f -> f matrix i))
                    (protocolTypeGetter |> Option.map (fun f -> f matrix i))
                    (protocolDescriptionGetter |> Option.map (fun f -> f matrix i))
                    (protocolURIGetter |> Option.map (fun f -> f matrix i))
                    (protocolVersionGetter |> Option.map (fun f -> f matrix i))
                    (parameters)
                    (componentGetters |> List.map (fun f -> f matrix i) |> Aux.Option.fromValueWithDefault [])
                    None

            Process.make 
                None 
                pn 
                (Some protocol) 
                (paramvalues)
                None
                None
                None
                None          
                (inputGetter matrix i |> Some)
                (outputGetter matrix i |> Some)
                None

    let groupProcesses (ps : Process list) = 
        ps
        |> List.groupBy (fun x -> 
            if x.Name.IsSome && (x.Name.Value |> Process.decomposeName |> snd).IsSome then
                (x.Name.Value |> Process.decomposeName |> fst)
            elif x.ExecutesProtocol.IsSome && x.ExecutesProtocol.Value.Name.IsSome then
                x.ExecutesProtocol.Value.Name.Value 
            elif x.Name.Value.Contains "_" then
                let lastUnderScoreIndex = x.Name.Value.LastIndexOf '_'
                x.Name.Value.Remove lastUnderScoreIndex
            else
                x.Name.Value           
        )

    let processToRows (p : Process) =
        let pvs = p.ParameterValues |> Option.defaultValue [] |> List.map (fun ppv -> JsonTypes.decomposeParameterValue ppv, ColumnIndex.tryGetParameterColumnIndex ppv)
        let components = 
            match p.ExecutesProtocol with
            | Some prot ->
                prot.Components |> Option.defaultValue [] |> List.map (fun ppv -> JsonTypes.decomposeComponent ppv, ColumnIndex.tryGetComponentIndex ppv)
            | None -> []
        let protVals = 
            match p.ExecutesProtocol with
            | Some prot ->
                [
                    if prot.Name.IsSome then CompositeHeader.ProtocolREF, CompositeCell.FreeText prot.Name.Value
                    if prot.ProtocolType.IsSome then CompositeHeader.ProtocolType, CompositeCell.Term prot.ProtocolType.Value
                    if prot.Description.IsSome then CompositeHeader.ProtocolDescription, CompositeCell.FreeText prot.Description.Value
                    if prot.Uri.IsSome then CompositeHeader.ProtocolUri, CompositeCell.FreeText prot.Uri.Value
                    if prot.Version.IsSome then CompositeHeader.ProtocolVersion, CompositeCell.FreeText prot.Version.Value
                ]
            | None -> []
        p.Outputs.Value
        |> List.zip p.Inputs.Value
        |> List.map (fun (i,o) ->
            let chars = i |> ProcessInput.getCharacteristicValues |> List.map (fun cv -> JsonTypes.decomposeCharacteristicValue cv, ColumnIndex.tryGetCharacteristicColumnIndex cv)
            let factors = o |> ProcessOutput.getFactorValues |> List.map (fun fv -> JsonTypes.decomposeFactorValue fv, ColumnIndex.tryGetFactorColumnIndex fv)
            let vals = 
                (chars @ components @ pvs @ factors)
                |> List.sortBy (snd >> Option.defaultValue 10000)
                |> List.map fst
            [
                yield JsonTypes.decomposeProcessInput i
                yield! protVals
                yield! vals
                yield JsonTypes.decomposeProcessOutput o
            ]
        )
        
    let compositeHeaderEqual (ch1 : CompositeHeader) (ch2 : CompositeHeader) = 
        ch1.ToString() = ch2.ToString()

    let alignByHeaders (vals : ((CompositeHeader * CompositeCell) list) list) = 
        let headers : ResizeArray<CompositeHeader> = ResizeArray()
        let values : Dictionary<int*int,CompositeCell> = Dictionary()
        let getFirstElem (vals : ('T list) list) : 'T =
            List.pick (fun l -> if List.isEmpty l then None else List.head l |> Some) vals
        let rec loop colI (vals : ((CompositeHeader * CompositeCell) list) list) =
            if List.exists (List.isEmpty >> not) vals then 
                headers,values
            else 
                let firstElem = vals |> getFirstElem |> fst
                headers.Add firstElem
                let vals = 
                    vals
                    |> List.mapi (fun rowI l ->
                        match l with
                        | [] -> []
                        | (h,c)::t ->
                            if compositeHeaderEqual h firstElem then
                                values.Add((colI,rowI),c)
                                t
                            else
                                l
                    )
                loop (colI+1) vals
        loop 0 vals
                
        