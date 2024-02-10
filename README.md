# ShoppingCartExamples Solution

Welcome to the ShoppingCartExamples solution, a simple example demonstrating various implementations of a simple shopping cart using Orleans in .NET. 

This solution was created to support the Temporal Talk in London ***Scaling Workflow Systems with Temporal, .NET, and Orleans: An Integrated Approach*** and includes three distinct variants. 

Each variant showcases different approaches to managing workflows, error handling, and resilience within a .NET application.

## Variants Overview

- **PureCSharp ShoppingCart Example**: Implements a basic shopping cart using standard C# and .NET features, focusing on simplicity and clarity.
- **Polly ShoppingCart Example**: Utilizes the Polly library to apply advanced resilience patterns like retries, circuit breakers, and timeout policies.
- **Temporal ShoppingCart Example**: Leverages Temporal.io's capabilities for orchestrating workflows, providing fault tolerance and seamless retries.

## Prerequisites

- .NET SDK
- Visual Studio or another compatible IDE
- Docker and Docker Compose (for the Temporal Workflow Demo)

## Getting Started

To begin, ensure you have cloned the repository and have the necessary prerequisites installed on your machine. 

1. **Open the Solution**:
    - Open Visual Studio.
    - Navigate to `File` > `Open` > `Project/Solution`.
    - Locate and select the `ShoppingCartExamples.sln` file.

2. **Restore Dependencies**:
    - Right-click on the solution in Solution Explorer.
    - Select "Restore NuGet Packages".

# Testing the ShoppingCart Example

Follow these steps to test the ShoppingCart Example using the Swagger UI. Ensure you use the same `cartId` for all the API actions during the test.

## Step 1: Add Products to Cart

Use the `/cart/add` endpoint to add products to your shopping cart.

- **Endpoint**: `POST /cart/add`
- **Query Parameter**: `cartId` (Choose any integer, e.g., 1)
- **Request Body**: Provide product details. You can use the default payload if you wish.
  
  Example payload:
  ```json
  {
    "name": "Example Product"
  }
  ```

## Step 2: View Cart Contents

After adding products, check the contents of your cart using the `/cart/view` endpoint.

- **Endpoint**: `GET /cart/view`
- **Query Parameter**: `cartId` (Use the same `cartId` as in Step 1)

This step verifies that the products have been successfully added to the cart.

## Step 3: Checkout

Finally, trigger the checkout workflow using the `/cart/checkout` endpoint.

- **Endpoint**: `POST /cart/checkout`
- **Query Parameter**: `cartId` (Use the same `cartId` as in previous steps)

This action will process the items in your cart and perform the checkout workflow.

## Accessing Swagger UI

To access the Swagger UI and perform these actions:

- Navigate to `https://localhost:59855/swagger/index.html` in your browser.
- Use the Swagger UI to interact with the above endpoints.

## Running the Variants

To run a specific variant, set it as the startup project:

1. **Temporal Workflow Demo**:
    - Navigate to the `ShoppingCartExample.Temporal` project.
    - Right-click and choose 'Set as Startup Project'.
    - Follow the project-specific README for running the Temporal server.

2. **PureCSharp ShoppingCart Example**:
    - Navigate to the `ShoppingCartExample.PureCSharp` project.
    - Right-click and choose 'Set as Startup Project'.
    - Start the application.

3. **Polly ShoppingCart Example**:
    - Navigate to the `ShoppingCartExample.Polly` project.
    - Right-click and choose 'Set as Startup Project'.
    - Start the application.

## Variant-Specific Instructions

Each variant has its own set of instructions and prerequisites:

- [Temporal Workflow Demo README](/ShoppingCartExample.Temporal/README.md)
- [PureCSharp ShoppingCart Example README](/ShoppingCartExample.PureCSharp/README.md)
- [Polly ShoppingCart Example README](/ShoppingCartExample.Polly/README.md)

Please refer to these README files for detailed instructions on running and interacting with each variant.

## Conclusion

The ShoppingCartExamples solution is designed to provide a hands-on look at different architectural approaches in .NET applications. Whether you're interested in workflow orchestration, fundamental C# implementations, or resilience patterns, this solution offers very basic insight.

## Further Enhancement

A more realistic scale implementation of the Temporal example including:

**Non-Blocking execution of the workflow** - typically this would return to the user to config acceptance of the order and the workflow activities would then drive interaction with the user as the workflow progressed.
