using FAM.Infrastructure.PersistenceModels.Mongo;

using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FAM.Infrastructure.Providers.MongoDB;

/// <summary>
/// MongoDB class maps and conventions setup
/// </summary>
public static class MongoClassMaps
{
    public static void Register()
    {
        // Register class maps
        if (!BsonClassMap.IsClassMapRegistered(typeof(CompanyMongo)))
            BsonClassMap.RegisterClassMap<CompanyMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(UserMongo)))
            BsonClassMap.RegisterClassMap<UserMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        // Authorization entities
        if (!BsonClassMap.IsClassMapRegistered(typeof(PermissionMongo)))
            BsonClassMap.RegisterClassMap<PermissionMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(RoleMongo)))
            BsonClassMap.RegisterClassMap<RoleMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(ResourceMongo)))
            BsonClassMap.RegisterClassMap<ResourceMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(RolePermissionMongo)))
            BsonClassMap.RegisterClassMap<RolePermissionMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(UserNodeRoleMongo)))
            BsonClassMap.RegisterClassMap<UserNodeRoleMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        // Organizations entities
        if (!BsonClassMap.IsClassMapRegistered(typeof(OrgNodeMongo)))
            BsonClassMap.RegisterClassMap<OrgNodeMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(CompanyDetailsMongo)))
            BsonClassMap.RegisterClassMap<CompanyDetailsMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(DepartmentDetailsMongo)))
            BsonClassMap.RegisterClassMap<DepartmentDetailsMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        // Asset Management entities
        if (!BsonClassMap.IsClassMapRegistered(typeof(SupplierMongo)))
            BsonClassMap.RegisterClassMap<SupplierMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
    }
}

/// <summary>
/// MongoDB indexes setup
/// </summary>
public static class MongoIndexes
{
    public static async Task CreateIndexesAsync(IMongoDatabase database)
    {
        // Companies collection indexes
        IMongoCollection<CompanyMongo>? companiesCollection = database.GetCollection<CompanyMongo>("companies");

        IndexKeysDefinitionBuilder<CompanyMongo>? indexKeys = Builders<CompanyMongo>.IndexKeys;
        var indexOptions = new CreateIndexOptions { Unique = false };

        // Name index
        await companiesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<CompanyMongo>(
                indexKeys.Text(c => c.Name),
                indexOptions));

        // TaxCode unique index (only for non-deleted documents)
        IndexKeysDefinition<CompanyMongo>? taxCodeIndex = Builders<CompanyMongo>.IndexKeys.Ascending(c => c.TaxCode);
        var taxCodeOptions = new CreateIndexOptions
        {
            Unique = true
        };
        // Note: Partial indexes require MongoDB 3.2+ and specific syntax
        // For now, we'll handle uniqueness in application logic
        await companiesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<CompanyMongo>(taxCodeIndex, taxCodeOptions));

        // DomainId index for cross-references
        await companiesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<CompanyMongo>(
                indexKeys.Ascending(c => c.DomainId),
                new CreateIndexOptions { Unique = true }));

        // Users collection indexes
        IMongoCollection<UserMongo>? usersCollection = database.GetCollection<UserMongo>("users");

        // Username index
        IndexKeysDefinition<UserMongo>? usernameIndex = Builders<UserMongo>.IndexKeys.Ascending(u => u.Username);
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserMongo>(usernameIndex, new CreateIndexOptions { Unique = true }));

        // Email index
        IndexKeysDefinition<UserMongo>? emailIndex = Builders<UserMongo>.IndexKeys.Ascending(u => u.Email);
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserMongo>(emailIndex, new CreateIndexOptions { Unique = true }));

        // DomainId index for cross-references
        await usersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserMongo>(
                Builders<UserMongo>.IndexKeys.Ascending(u => u.DomainId),
                new CreateIndexOptions { Unique = true }));

        // Authorization collections indexes
        IMongoCollection<PermissionMongo>? permissionsCollection =
            database.GetCollection<PermissionMongo>("permissions");
        IndexKeysDefinition<PermissionMongo>? permissionsIndex =
            Builders<PermissionMongo>.IndexKeys.Ascending(p => p.Resource).Ascending(p => p.Action);
        await permissionsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<PermissionMongo>(permissionsIndex, new CreateIndexOptions { Unique = true }));

        IMongoCollection<RoleMongo>? rolesCollection = database.GetCollection<RoleMongo>("roles");
        IndexKeysDefinition<RoleMongo>? roleCodeIndex = Builders<RoleMongo>.IndexKeys.Ascending(r => r.Code);
        await rolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<RoleMongo>(roleCodeIndex, new CreateIndexOptions { Unique = true }));

        IndexKeysDefinition<RoleMongo>? roleRankIndex = Builders<RoleMongo>.IndexKeys.Ascending(r => r.Rank);
        await rolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<RoleMongo>(roleRankIndex));

        IMongoCollection<ResourceMongo>? resourcesCollection = database.GetCollection<ResourceMongo>("resources");
        IndexKeysDefinition<ResourceMongo>? resourceIndex =
            Builders<ResourceMongo>.IndexKeys.Ascending(r => r.Type).Ascending(r => r.NodeId);
        await resourcesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<ResourceMongo>(resourceIndex));

        IMongoCollection<RolePermissionMongo>? rolePermissionsCollection =
            database.GetCollection<RolePermissionMongo>("role_permissions");
        IndexKeysDefinition<RolePermissionMongo>? rolePermissionIndex = Builders<RolePermissionMongo>.IndexKeys
            .Ascending(rp => rp.RoleId)
            .Ascending(rp => rp.PermissionId);
        await rolePermissionsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<RolePermissionMongo>(rolePermissionIndex, new CreateIndexOptions { Unique = true }));

        IMongoCollection<UserNodeRoleMongo>? userNodeRolesCollection =
            database.GetCollection<UserNodeRoleMongo>("user_node_roles");
        IndexKeysDefinition<UserNodeRoleMongo>? userNodeRoleIndex = Builders<UserNodeRoleMongo>.IndexKeys
            .Ascending(unr => unr.UserId)
            .Ascending(unr => unr.NodeId).Ascending(unr => unr.RoleId);
        await userNodeRolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<UserNodeRoleMongo>(userNodeRoleIndex, new CreateIndexOptions { Unique = true }));

        // Organizations collections indexes
        IMongoCollection<OrgNodeMongo>? orgNodesCollection = database.GetCollection<OrgNodeMongo>("org_nodes");
        IndexKeysDefinition<OrgNodeMongo>? orgNodeParentIndex =
            Builders<OrgNodeMongo>.IndexKeys.Ascending(n => n.ParentId);
        await orgNodesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<OrgNodeMongo>(orgNodeParentIndex));

        IndexKeysDefinition<OrgNodeMongo>? orgNodeTypeIndex = Builders<OrgNodeMongo>.IndexKeys.Ascending(n => n.Type);
        await orgNodesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<OrgNodeMongo>(orgNodeTypeIndex));

        IMongoCollection<CompanyDetailsMongo>? companyDetailsCollection =
            database.GetCollection<CompanyDetailsMongo>("company_details");
        IndexKeysDefinition<CompanyDetailsMongo>? companyNodeIndex =
            Builders<CompanyDetailsMongo>.IndexKeys.Ascending(cd => cd.NodeId);
        await companyDetailsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<CompanyDetailsMongo>(companyNodeIndex, new CreateIndexOptions { Unique = true }));

        IndexKeysDefinition<CompanyDetailsMongo>? companyTaxCodeIndex =
            Builders<CompanyDetailsMongo>.IndexKeys.Ascending(cd => cd.TaxCode);
        await companyDetailsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<CompanyDetailsMongo>(companyTaxCodeIndex));

        IndexKeysDefinition<CompanyDetailsMongo>? companyDomainIndex =
            Builders<CompanyDetailsMongo>.IndexKeys.Ascending(cd => cd.Domain);
        await companyDetailsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<CompanyDetailsMongo>(companyDomainIndex));

        IMongoCollection<DepartmentDetailsMongo>? departmentDetailsCollection =
            database.GetCollection<DepartmentDetailsMongo>("department_details");
        IndexKeysDefinition<DepartmentDetailsMongo>? departmentNodeIndex =
            Builders<DepartmentDetailsMongo>.IndexKeys.Ascending(dd => dd.NodeId);
        await departmentDetailsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DepartmentDetailsMongo>(departmentNodeIndex,
                new CreateIndexOptions { Unique = true }));

        IndexKeysDefinition<DepartmentDetailsMongo>? departmentCostCenterIndex =
            Builders<DepartmentDetailsMongo>.IndexKeys.Ascending(dd => dd.CostCenter);
        await departmentDetailsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<DepartmentDetailsMongo>(departmentCostCenterIndex));

        // Suppliers collection indexes
        IMongoCollection<SupplierMongo>? suppliersCollection = database.GetCollection<SupplierMongo>("suppliers");

        // Code unique index
        IndexKeysDefinition<SupplierMongo>?
            supplierCodeIndex = Builders<SupplierMongo>.IndexKeys.Ascending(s => s.Code);
        await suppliersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<SupplierMongo>(supplierCodeIndex, new CreateIndexOptions { Unique = true }));

        // Name index
        IndexKeysDefinition<SupplierMongo>? supplierNameIndex = Builders<SupplierMongo>.IndexKeys.Text(s => s.Name);
        await suppliersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<SupplierMongo>(supplierNameIndex));

        // ContactEmail unique index
        IndexKeysDefinition<SupplierMongo>? supplierEmailIndex =
            Builders<SupplierMongo>.IndexKeys.Ascending(s => s.ContactEmail);
        await suppliersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<SupplierMongo>(supplierEmailIndex, new CreateIndexOptions { Unique = true }));

        // CountryId index
        IndexKeysDefinition<SupplierMongo>? supplierCountryIndex =
            Builders<SupplierMongo>.IndexKeys.Ascending(s => s.CountryId);
        await suppliersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<SupplierMongo>(supplierCountryIndex));

        // IsActive index
        IndexKeysDefinition<SupplierMongo>? supplierActiveIndex =
            Builders<SupplierMongo>.IndexKeys.Ascending(s => s.IsActive);
        await suppliersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<SupplierMongo>(supplierActiveIndex));

        // DomainId index for cross-references
        await suppliersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<SupplierMongo>(
                Builders<SupplierMongo>.IndexKeys.Ascending(s => s.DomainId),
                new CreateIndexOptions { Unique = true }));
    }
}
