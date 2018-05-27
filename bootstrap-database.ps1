$downloadUrl = "https://s3-us-west-2.amazonaws.com/dynamodb-local/dynamodb_local_latest.zip"
$fileName = "dynamoDb.zip"
$zipFileLocation = "$pwd\$filename"
$localDbFolder = "$pwd\DynamoDb"
$port = 5153
$javaLibraryPath = "$localDbFolder\DynamoDBLocal_lib"
$jarPath = "$localDbFolder\DynamoDBLocal.jar"
$projectRoot = $pwd


Write-Output "Checking Local Db Folder: $localDbFolder"
if(Test-Path $localDbFolder){ 
    Write-Output "Local Db Folder exists...skipping initialization..."
}else{
    Write-Output "Local Db Folder not found, initializing local DynamoDb instance..."
    Write-Output "Installing DynamoDb Locally..."

    Write-Output "Checking for $filename..."
    if(Test-Path $fileName){
        Write-Output "Zip file already exists...skipping download"
    }else{
        Write-Output "Downloading Local DynamoDb installer..."
        Write-Output "Download Url: $downloadUrl"
        Write-Output "Download Location: $zipFileLocation"
        wget $downloadUrl -OutFile $zipFileLocation        
    }

    Write-Output "Creating directory $locaDbFolder..."
    New-Item -ItemType Directory $localDbFolder

    Write-Output "Extracting zip contents at $zipFileLocation..."
    Add-Type -assembly �system.io.compression.filesystem�
	Expand-Archive $zipFileLocation -DestinationPath $localDbFolder    
}


Write-Output "Checking port $port..."
$portStatus = Get-NetTCPConnection -State Listen | Where-Object { $_.LocalPort -eq $port}
$seed = 0
If([string]::IsNullOrEmpty($portStatus)) {
	Write-Output "Port is available..."
	Write-Output "Starting DynamoDb instance on port $port"
	$seed = 1
	Write-Output "Java lib path: $javaLibraryPath"
	Write-Output "Jar path: $jarPath"
	javaw -D"java.library.path"=./$javaLibraryPath -jar $jarPath -sharedDb -port $port -inMemory
	Write-Output "DynamoDb instance ready..."
} 
Else {
	Write-Output $portStatus
	Write-Output "Port is in use. Assumption is that local DynamoDb is already running..."
	Write-Output "Killing local DynamoDb..."
	Stop-Process -processname javaw
	$seed = 1
	Write-Output "Starting DynamoDb instance on port $port"	
	Write-Output "Java lib path: $javaLibraryPath"
	Write-Output "Jar path: $jarPath"
	javaw -D"java.library.path"=./$javaLibraryPath -jar $jarPath -sharedDb -port $port -inMemory
	Write-Output "DynamoDb instance ready..."
}


$projectPath = "$pwd\AdjacencyListDemo.Web"
$project = "$projectPath\AdjacencyListDemo.Web.csproj"

Write-Output "Bootstrapping database..."
dotnet run --project $project -- bootstrapdb