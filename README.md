<div style="margin-bottom: 1%; padding-bottom: 2%;">
	<img align="right" width="100px" src="https://dx29.ai/assets/img/logo-Dx29.png">
</div>

Dx29 Exomiser
===

### **Overview**

This project is a wrapped infrastructure to execute [Exomiser](https://github.com/exomiser/Exomiser), a tool to annotate and prioritize exome variants.

Dx29 Exomiser is a microservice used in [Dx29 Application](https://dx29.ai/) to extract exome variants from .vcf files. Since the process of extracting such variants is a time consuming task, this microservice enable the execution of Exomiser asyncroniously, based on a message queue infrastructure.

It is programmed in C#, and the structure of the project is as follows:

>- src folder: This is made up of multiple folders which contains the source code of the project.
>>- **Dx29.Exomiser.WebAPI**. This project expose the web API methods to process .vcf files.
>>- **Dx29.Exomiser**. This project contains the core logic to prepare input data and invoke Exomiser to extract the variants.
>>- **Dx29.Exomiser.Worker**. This project manages the Dispatcher required for the asynchronous functionalities. The administration and communication with the ServiceBus is carried out in this project.
>>- **Dx29**, **Dx29.Azure** and **Dx29.Jobs** used as libraries to add the common or more general functionalities used in Dx29 projects programmed in C#.
>- .gitignore file
>- README.md file
>- manifests folder: with the YAML configuration files for deploy in Azure Container Registry and Azure Kubernetes Service.
>- pipeline sample YAML file. For automatizate the tasks of build and deploy on Azure.

<p>&nbsp;</p>

### **Getting Started**
In order to execute Exomiser, this project relays on a docker image with the Exomiser binaries installed.

To bild the docker image, follow these steps:
1. Clone this repository
2. Go to the [Exomiser release repository](https://github.com/exomiser/Exomiser/releases) and download the binaries for version exomiser-cli-12.1.0
3. Copy the binary files into the Exomiser/exomiser directory
4. Enter the 'Exomiser' folder
5. Execute the following docker command inside the Exomiser directory:

```
docker build -t exomiser-v12.1.0.2 .
```

#### Pre-requisites


####  1. Configuration:

The project must include a configuration file: appsettings.json. This includes the dependencies with other microservices:

|  Key                 | Value               |		                                                                                |
|----------------------|---------------------|--------------------------------------------------------------------------------------|
| ExomiserConnectionStrings    | BlobStorage         |Blob Storage connection string                                                         |
| ExomiserServiceBus           | ConnectionString    |Message Queue connection string |                                                         |
| ExomiserQueueName           | QueueName           |Queue configured name                                                                 

<p>&nbsp;</p>

####  2. Download and installation

Download the repository code with `git clone` or use download button.

We use [Visual Studio 2019](https://docs.microsoft.com/en-GB/visualstudio/ide/quickstart-aspnet-core?view=vs-2022) for working with this project.

<p>&nbsp;</p>

####  3. Latest releases

The latest release of the project deployed in the [Dx29 application](https://dx29.ai/) is: 
>- Dx29.Annotations: v0.12.00.
>- Dx29.AnnotationsJobs: v0.12.00.

<p>&nbsp;</p>

#### 4. API references

>- PUT Process
>>- Start a new job to process a .vcf file given a UserId, CaseId and ResourceId of the .vcf file to extract the variants.
>>- PUT request: ```/api/v1/Exomiser/process```
>>- Body request: See ExomiserRequest class to see details of implementation.
>>- Response: Job Status
>>>- Name
>>>- Token: Job identifier. This will be needed later for job status and/or result queries.
>>>- Date: Date when the query is performed
>>>- Status: String indicating the status of the job. Can be: Failed, Succeeded, Running, Preparing, Pending, Created or Unknown.
>>>- CreatedOn: Date when the operation has made.
>>>- LastUpdate
>- POST Process
>>- Start a new job to process the files sent directly in the post.
>>- POST request: ```/api/v1/Exomiser/process```
>>- Body request: Streamed files:
>>>- requestInput: A serialized instance of the ExomiserRequest class with the information needed by Exomiser.
>>>- files: Streamed files to be processed:
>>>>- .vcf file (required)
>>>>- .ped file (optional)
>>- Response: Job status. An object like the one in the request described above is returned.
>- GET Status 
>>- To request the status of a given job. The identifier of the job corresponds to the token returned by any of the previous requests.
>>- GET request: ```/api/v1/Exomiser/status?params=<token>```
>>- Response: Job status. An object like the one in the request described above is returned.
>- GET Results
>>- To obtain the results of a given job. The job identifier corresponds to the token returned by any of the previous requests. 
>>- GET request: ```/api/v1/Exomiser/results?params=<token>```
>>- Json string with the results of the Exomiser execution. Refers [The Exomiser project](https://github.com/exomiser/Exomiser) documentation to know how to interpret these results.

<p>&nbsp;</p>

### **Build and Test**

#### 1. Build

We could use Docker. 

Docker builds images automatically by reading the instructions from a Dockerfile – a text file that contains all commands, in order, needed to build a given image.

>- A Dockerfile adheres to a specific format and set of instructions.
>- A Docker image consists of read-only layers each of which represents a Dockerfile instruction. The layers are stacked and each one is a delta of the changes from the previous layer.

Consult the following links to work with Docker:

>- [Docker Documentation](https://docs.docker.com/reference/)
>- [Docker get-started guide](https://docs.docker.com/get-started/overview/)
>- [Docker Desktop](https://www.docker.com/products/docker-desktop)

The first step is to run docker image build. We pass in . as the only argument to specify that it should build using the current directory. This command looks for a Dockerfile in the current directory and attempts to build a docker image as described in the Dockerfile. 
```docker image build . ```

[Here](https://docs.docker.com/engine/reference/commandline/docker/) you can consult the Docker commands guide.

<p>&nbsp;</p>

#### 2. Deployment

To work locally, it is only necessary to install the project and build it using Visual Studio 2019. 

The deployment of this project in an environment is described in [Dx29 architecture guide](https://dx29-v2.readthedocs.io/en/latest/index.html), in the deployment section. In particular, it describes the steps to execute to work with this project as a microservice (Docker image) available in a kubernetes cluster:

1. Create an Azure container Registry (ACR). A container registry allows you to store and manage container images across all types of Azure deployments. You deploy Docker images from a registry. Firstly, we need access to a registry that is accessible to the Azure Kubernetes Service (AKS) cluster we are creating. For this purpose, we will create an Azure Container Registry (ACR), where we will push images for deployment.
2. Create an Azure Kubernetes cluster (AKS) and configure it for using the prevouos ACR
3. Import image into Azure Container Registry
4. Publish the application with the YAML files that defines the deployment and the service configurations. 

This project includes, in the Deployments folder, YAML examples to perform the deployment tasks as a microservice in an AKS. 

Note that this service is configured as "ClusterIP" since it is not exposed externally in the [Dx29 application](https://dx29.ai/), but is internal for the application to use. If it is required to be visible there are two options:
>- The first, as realised in the Dx29 project an API is exposed that communicates to third parties with the microservice functionality.
>- The second option is to directly expose this microservice as a LoadBalancer and configure a public IP address and DNS.

**Interesting link**: [Deploy a Docker container app to Azure Kubernetes Service](https://docs.microsoft.com/en-GB/azure/devops/pipelines/apps/cd/deploy-aks?view=azure-devops&tabs=java)

<p>&nbsp;</p>


### **Contribute**

Please refer to each project's style and contribution guidelines for submitting patches and additions. The project uses [gitflow workflow](https://nvie.com/posts/a-successful-git-branching-model/). 
According to this it has implemented a branch-based system to work with three different environments. Thus, there are two permanent branches in the project:
>- The develop branch to work on the development environment.
>- The master branch to work on the test and production environments.

In general, we follow the "fork-and-pull" Git workflow.

>1. Fork the repo on GitHub
>2. Clone the project to your own machine
>3. Commit changes to your own branch
>4. Push your work back up to your fork
>5. Submit a Pull request so that we can review your changes

The project is licenced under the **(TODO: LICENCE & LINK & Brief explanation)**

<p>&nbsp;</p>
<p>&nbsp;</p>

<div style="border-top: 1px solid !important;
	padding-top: 1% !important;
    padding-right: 1% !important;
    padding-bottom: 0.1% !important;">
	<div align="right">
		<img width="150px" src="https://dx29.ai/assets/img/logo-foundation-twentynine-footer.png">
	</div>
	<div align="right" style="padding-top: 0.5% !important">
		<p align="right">	
			Copyright © 2020
			<a style="color:#009DA0" href="https://www.foundation29.org/" target="_blank"> Foundation29</a>
		</p>
	</div>
<div>
