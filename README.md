# RdpAttackNotificator
Service for notification of RDP attacks

# Setup config
All configurations store on RdpAttackNotificator.Service.exe.config


Service can by configured for each type of OS. If current version was not found in OS list, default record will use.
Config has already contains search pattern for 'ru' and 'us' version of OS.
```C#
<OperationSystems>
    <OperationSystem Version="Microsoft Windows NT 6.2.9200.0" InstanceId="4625" Source="Security" IsDefault="true">
        <SearchPatterns>
            <add name="us" value="Account For Which Logon Failed.+Account Name:\s+(?&lt;Login&gt;[\d\w\.]+).+Source Network Address:\s+(?&lt;SourceIp&gt;[\d\.]+)"/>
            <add name="ru" value="Учетная запись, которой не удалось выполнить вход.+Имя учетной записи:\s+(?&lt;Login&gt;[\d\w\.]+).+Сетевой адрес источника:\s+(?&lt;SourceIp&gt;[\d\.]+)"/>
        </SearchPatterns>
    </OperationSystem>
</OperationSystems>
```
Service allow to send black list to set of routers. By default it has one record for mikrotik type device.    
```C#
<LockTargets>
    <LockTarget Index="1" TargetType="Mikrotik" Target="192.168.88.1" Port="8728" User="admin" Password="" BlackList="rdp_restricted" LockPeriod="1d" />
</LockTargets>
```
Service allow skip IPs accourding to white list.
```C#
<WhiteList>
    <add name="local" value="127.0.0.1"/>
</WhiteList>
```
