# Lore

Worldbuilding inside the Unity Editor. Lore organizes your game's lore in **Categories**, **Entries** and
**Fields**, links Entries in descriptions, keeps an eye on integrity and content health, and lets your game
read everything at runtime through a small read-only API.

## Installation

Package Manager > + > *Install package from git URL…* >
`https://github.com/PebblePouch/Lore.git?path=/Packages/com.pebblepouch.lore`

This follows the latest state of `main`. To pin a version, append its tag (e.g. `#0.1.1`); all versions are listed
under [Tags](https://github.com/PebblePouch/Lore/tags).

Tested with Unity 6000.2 and 6000.6. Other Unity 6 versions are supported on a best-effort basis.

Before updating Lore or Unity, commit your project to version control or back up your Lore data (the data folder,
`Assets/Lore` by default, and your Libraries).

## Documentation

- [Package README](Packages/com.pebblepouch.lore/README.md): getting started and features
- [Runtime API](Packages/com.pebblepouch.lore/Documentation~/RuntimeAPI.md): reading Lore data in your game
- [CSV export & import](Packages/com.pebblepouch.lore/Documentation~/CSV.md)
- [Changelog](Packages/com.pebblepouch.lore/CHANGELOG.md)

## Repository

The package lives in `Packages/com.pebblepouch.lore`; the rest of the repository is the Unity project used to
develop it.

## License

Lore is **free for noncommercial use** under the [PolyForm Noncommercial License 1.0.0](LICENSE.md). Commercial use
needs the [Lore Commercial License](Packages/com.pebblepouch.lore/LICENSE-COMMERCIAL.md), which comes with the
**Supporter** tier on [Patreon](https://www.patreon.com/c/pebblepouch).

| Use | License |
|---|---|
| Personal study, hobby projects, prototypes without commercial plans | Free |
| Free games, even if players can tip you or support you on Patreon | Free |
| Pay-what-you-want releases where paying is optional (minimum price 0) | Free |
| Game jam entries, even if the jam awards prizes | Free |
| Schools, universities, charities, public institutions | Free |
| Games or products you sell (paid release, Early Access, DLC, in-app purchases) | Commercial |
| Work for a company or for paying clients | Commercial |
| Companies evaluating Lore (30 days, nothing released) | Free |

A hobby project that turns commercial needs the Commercial License before its first commercial release. One
Supporter pledge covers teams and companies of up to 10 people; larger teams please get in touch.
These terms apply to all versions of Lore, including 0.1.0, which was published before the license was chosen.
Free projects that ship Lore must pass on the license notice (the `Required Notice` line and the license URL from
[LICENSE.md](LICENSE.md)), e.g. in a credits screen or a text file next to the build.

Questions or other arrangements: pebblepouchproductions@gmail.com

Contributions (pull requests) are not accepted at the moment.

## Support

If Lore helps you, you can support its development on [Patreon](https://www.patreon.com/c/pebblepouch).
