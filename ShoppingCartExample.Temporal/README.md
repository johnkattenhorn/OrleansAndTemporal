# Temporal Workflow Demo

This project demonstrates a Temporal workflow for a shopping cart application. It illustrates how Temporal can be used to orchestrate complex workflows in a distributed system.

## Prerequisites

- Docker and Docker Compose
- .NET SDK
- Visual Studio

## Setting Up the Temporal Server

To run the Temporal server locally, you can use Docker Compose. (There are other ways to run the development environment locally)

1. **Clone the Temporal Docker Compose repository**:

    ```bash
    git clone https://github.com/temporalio/docker-compose.git
    cd docker-compose
    ```

2. **Start the Temporal server**:

    ```bash
    docker-compose up
    ```

   This command starts the Temporal server and its dependencies (such as Cassandra or MySQL, depending on the configuration).

3. Creating an Alias for Temporal CLI in PowerShell

To simplify the usage of the Temporal CLI within the Docker container, you can create an alias in PowerShell. This alias allows you to run Temporal CLI commands without typing the full Docker command each time.

1. **Open PowerShell**:

    Start PowerShell on your machine.

2. **Define the Alias**:

    Run the following command in PowerShell to create an alias named `TemporalCli`:

    ```powershell
    function TemporalCli { docker exec -it temporal-admin-tools temporal $args }
    Set-Alias -Name tctl -Value TemporalCli
    ```

    This command creates a function `TemporalCli` that runs Temporal CLI commands inside the Docker container (`temporal-admin-tools`). The `Set-Alias` command then creates an alias `tctl` that points to this function.

3. **Using the Alias**:

    Now, you can use the `tctl` alias in PowerShell to run Temporal CLI commands. For example:

    ```powershell
    tctl namespace list
    ```

    This command will list all Temporal namespaces using the Temporal CLI within the Docker container.

4. **Making the Alias Persistent** (Optional):

    If you want this alias to be available in all future PowerShell sessions:

    - Open your PowerShell profile script:

      ```powershell
      notepad $PROFILE
      ```

    - Add the function and alias commands to this script:

      ```powershell
      function TemporalCli { docker exec -it temporal-admin-tools temporal $args }
      Set-Alias -Name tctl -Value TemporalCli
      ```

    - Save the file and restart PowerShell.

## Viewing Workflow History in Temporal Web UI

The Temporal Web UI provides a visual interface to view workflow histories, see running workflows, and query workflow data.

1. **Access the Temporal Web UI**:

    The Temporal Web UI can be accessed at `http://localhost:8088`.

2. **Navigate to the Dashboard**:

    In the Temporal Web UI, you can navigate to the dashboard to view a list of namespaces and workflows.

3. **View Workflow Details**:

    Click on a specific workflow to view its details, history, and events. This is useful for debugging and understanding the behavior of your workflows.

## Conclusion

This README provides basic instructions to get started with the Temporal workflow demo. For more detailed information about Temporal and its capabilities, visit the [Temporal Documentation](https://docs.temporal.io/docs), to Temporal DotNet specific documentation, visit the SDK [Temporal DotNet SDK](https://github.com/temporalio/sdk-dotnet)