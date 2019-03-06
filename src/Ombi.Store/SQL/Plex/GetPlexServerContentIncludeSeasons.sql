SELECT * FROM PlexServerContent psc
JOIN PlexSeasonsContent seasons on psc.Key = seasons.ParentKey
Where psc.Key = @Key