## ADDED Requirements

### Requirement: Json-typed table cells render collapsed by default
The `Table` visualization SHALL render a cell whose column type is `QueryResultColumnType.Json` in a
collapsed summary form by default (e.g. `{...}` for an object-shaped value, `[...]` for an
array-shaped value), rather than printing the value's raw JSON text inline.

#### Scenario: An object-shaped Json cell shows a collapsed summary
- **WHEN** a table column's type is `QueryResultColumnType.Json` and a row's value for that column is
  an object
- **THEN** the cell displays a collapsed summary rather than the full raw JSON text

#### Scenario: An array-shaped Json cell shows a collapsed summary
- **WHEN** a table column's type is `QueryResultColumnType.Json` and a row's value for that column is
  an array
- **THEN** the cell displays a collapsed summary rather than the full raw JSON text

#### Scenario: A null Json-typed value renders without collapse controls
- **WHEN** a table column's type is `QueryResultColumnType.Json` and a row's value for that column is
  null
- **THEN** the cell renders as empty/null, with no expand affordance shown

### Requirement: Collapsed Json cells expand into a tree view on click
A collapsed `Json`-typed cell SHALL be expandable by user interaction into a tree/node view showing the
value's full nested structure, and SHALL be collapsible back to its summary form.

#### Scenario: Clicking a collapsed cell reveals its structure
- **WHEN** a user clicks a collapsed `Json`-typed cell
- **THEN** the cell expands to show a tree/node view of the value's full nested contents

#### Scenario: Clicking an expanded cell collapses it again
- **WHEN** a user clicks an already-expanded `Json`-typed cell
- **THEN** the cell returns to its collapsed summary form
