﻿namespace ISA


type ArcTable = 
    {
        Name : string
        ValueHeaders : CompositeHeader []
        /// Key: Column * Row
        Values : System.Collections.Generic.Dictionary<int*int,CompositeCell>  
    }

    /// Create ArcTable with empty 'ValueHeader' and 'Values' 
    static member init(name: string) = {
        Name = name
        ValueHeaders = [||]
        Values = System.Collections.Generic.Dictionary<int*int,CompositeCell>()
    }

    member this.ColumnCount = this.ValueHeaders.Length
    member this.RowCount = 
        if this.Values.Count = 0 then 0 else
            this.Values.Keys |> Seq.groupBy fst |> Seq.map (fun (c,r) -> Seq.length r) |> Seq.max

    member this.addColumn (header:CompositeHeader, ?cells: CompositeCell [], ?index: int) = 
        let cells = Option.defaultValue [||] cells
        let column = Array.singleton <| CompositeColumn.create(header, cells)
        this.addColumns (column, ?index = index)

    member this.addColumns (columns: CompositeColumn [], ?index: int) = 
        let index = Option.defaultValue this.ColumnCount index
        if index < 0 then failwith "Cannot insert CompositeColumn at index < 0."
        if index > this.ColumnCount then failwith $"Specified index is out of table range! Table contains only {this.ColumnCount} columns."
        columns |> Array.iter (fun x -> x.validate(true) |> ignore)
        let nNewColumn = columns.Length
        let columnHeaders, columnRows = columns |> Array.map (fun x -> x.Header, x.Cells) |> Array.unzip
        let nextHeaders = 
            let ra = ResizeArray(this.ValueHeaders)
            ra.InsertRange(index, columnHeaders)
            ra.ToArray()
        let maxRows = 
            let maxRows = columnRows |> Array.map (fun x -> x.Length) |> Array.max
            System.Math.Max(this.RowCount, maxRows)
        let nextBody =
            seq {
                for i, cells in (Array.indexed columnRows) do
                    let columnIndex = i + index
                    for rowIndex,v in (Array.indexed cells) do
                        yield 
                            (columnIndex,rowIndex),v
                for KeyValue((columnIndex,rowIndex),v) in this.Values do
                    let nextColumnIndex = if columnIndex >= index then columnIndex + nNewColumn else columnIndex
                    yield 
                        (nextColumnIndex,rowIndex),v
            }
            |> Seq.groupBy(fun ((columnIndex,_),_) -> columnIndex )
            |> Seq.collect (fun (columnKey, v) ->
                let nExistingRows = Seq.length v
                let diff = maxRows - nExistingRows
                let ra = ResizeArray(v)
                let relatedHeader = nextHeaders.[columnKey]
                let empty = if relatedHeader.IsTermColumn then CompositeCell.emptyTerm else CompositeCell.emptyFreeText
                let fillerCells = Array.init diff (fun i -> 
                    let nextRowIndex = i + nExistingRows // nExistingRows -> length(!) we keep length as we would need to add 1 anyways to work with the `i` zero based index;  i -> [0,diff];  
                    (columnKey, nextRowIndex), empty
                )
                ra.AddRange(fillerCells)
                ra.ToArray()
            )
            |> Map.ofSeq
            |> fun m -> System.Collections.Generic.Dictionary<int*int,CompositeCell>(m)
        { this with ValueHeaders = nextHeaders; Values = nextBody }
        

    static member insertParameterValue (t : ArcTable) (p : ProcessParameterValue) : ArcTable = 
        raise (System.NotImplementedException())

    static member getParameterValues (t : ArcTable) : ProcessParameterValue [] = 
        raise (System.NotImplementedException())

    // no 
    static member addProcess = 
        raise (System.NotImplementedException())

    static member addRow input output values = //yes
        raise (System.NotImplementedException())

    static member getProtocols (t : ArcTable) : Protocol [] = 
        raise (System.NotImplementedException())

    static member getProcesses (t : ArcTable) : Process [] = 
        raise (System.NotImplementedException())

    static member fromProcesses (ps : Process array) : ArcTable = 
        raise (System.NotImplementedException())
